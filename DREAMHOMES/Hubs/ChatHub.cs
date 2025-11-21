using AutoMapper;
using DREAMHOMES.Controllers.DTOs.ChatHub;
using DREAMHOMES.Models;
using DREAMHOMES.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace DREAMHOMES.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private static ConcurrentDictionary<string, UserConnection> _connections = new();
        private static ConcurrentDictionary<string, WaitingUserInfo> _userQueue = new();

        public ChatHub(IChatService chatService, IUserService userService, IMapper mapper)
        {
            _chatService = chatService;
            _userService = userService;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            // Get user info from JWT claims
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            var connectionId = Context.ConnectionId;

            // Check if user is an Agent (from JWT role claim)
            var isAgent = Context.User?.IsInRole("Agent") ?? false;

            if (string.IsNullOrEmpty(userId))
            {
                Context.Abort();
                return;
            }

            var isOnline = await this._userService.IsAgentOnline(userId);
            _connections[connectionId] = new UserConnection
            {
                UserId = userId,
                Email = email,
                ConnectionId = connectionId,
                IsAgent = isAgent,
                IsOnline = isOnline,
                ConnectedAt = DateTime.UtcNow
            };

            if (isAgent)
            {
                // Agent connected
                await Groups.AddToGroupAsync(connectionId, "Agents");
                await this._userService.UpdateUserStatus(userId, true);
                await Clients.Caller.SendAsync("AgentConnected", new
                {
                    userId,
                    email
                });
                await SyncQueueToAllAgents();
            }
            else
            {
                // Regular user connected - add to queue or assign agent
                await AssignAgentToUser(userId, email, connectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;

            if (_connections.TryRemove(connectionId, out var connection))
            {
                if (connection.IsAgent)
                {
                    await Groups.RemoveFromGroupAsync(connectionId, "Agents");
                    await _userService.UpdateUserStatus(connection.UserId, false);
                    await ReassignUsersFromAgent(connection.UserId);
                }
                else
                {
                    _userQueue.TryRemove(connection.UserId, out _);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string conversationId, string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAgent = Context.User?.IsInRole("Agent") ?? false;

            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized");
                return;
            }

            // Verify user is part of this conversation
            var conversation = await _chatService.GetConversation(conversationId);
            if (conversation == null ||
                (conversation.UserId != userId && conversation.AgentId != userId))
            {
                await Clients.Caller.SendAsync("Error", "Not authorized for this conversation");
                return;
            }

            // Save message to database
            var savedMessage = await _chatService.CreateMessage(
               userId,
               conversationId,
               message,
               DateTime.UtcNow,
               isAgent);

            // Get recipient
            var recipientId = isAgent ? conversation.UserId : conversation.AgentId;

            // Send to the other party
            await Clients.User(recipientId).SendAsync("ReceiveMessage", this._mapper.Map<ChatMessageDTO>(savedMessage));

            // Confirm to sender
            await Clients.Caller.SendAsync("MessageSent", this._mapper.Map<ChatMessageDTO>(savedMessage));
        }

        [Authorize(Roles = "Agent")] // Only agents can accept chats
        public async Task AgentAcceptChat(string waitingUserId)
        {
            var agentId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var agentEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(agentId))
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized");
                return;
            }

            // Check if agent has reached max concurrent chats
            var agentLoad = await _chatService.GetAgentCurrentLoad(agentId);
            var maxChats = await _userService.GetAgentMaxChats(agentId);

            if (agentLoad >= maxChats)
            {
                await Clients.Caller.SendAsync("Error", "Maximum concurrent chats reached");
                return;
            }

            // Create conversation
            var conversation = await _chatService.CreateConversation(waitingUserId, agentId);

            // Remove user from queue
            _userQueue.TryRemove(waitingUserId, out _);
           
            // Get agent name
            var agentName = await _userService.GetUserName(agentId);

            // Notify user they're connected to an agent
            await Clients.User(waitingUserId).SendAsync("AgentAssigned", new UserConversationDTO
            {
                ConversationId = conversation.Id,
                UserId = agentId,
                UserName = agentName ?? agentEmail,
                UserEmail = agentEmail
            });

            // Notify agent
            var userName = await _userService.GetUserName(waitingUserId);
            var userEmail = await _userService.GetUserEmail(waitingUserId);

            await Clients.Caller.SendAsync("ChatAccepted", new UserConversationDTO
            {
                ConversationId = conversation.Id,
                UserId = waitingUserId,
                UserName = userName,
                UserEmail = userEmail
            });

            await SyncQueueToAllAgents();
        }

        public async Task SendTypingIndicator(string conversationId, bool isTyping)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAgent = Context.User?.IsInRole("Agent") ?? false;

            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var conversation = await _chatService.GetConversation(conversationId);
            if (conversation == null)
            {
                return;
            }

            // Verify user is part of conversation
            if (conversation.UserId != userId && conversation.AgentId != userId)
            {
                return;
            }

            var recipientId = isAgent ? conversation.UserId : conversation.AgentId;

            await Clients.User(recipientId).SendAsync("UserTyping", new
            {
                userId,
                isTyping
            });
        }

        public async Task EndConversation(string conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var conversation = await _chatService.GetConversation(conversationId);

            // Verify user is part of conversation
            if (conversation == null ||
                (conversation.UserId != userId && conversation.AgentId != userId))
            {
                await Clients.Caller.SendAsync("Error", "Not authorized");
                return;
            }

            await _chatService.EndConversation(conversationId);

            var otherUserId = conversation.UserId == userId ? conversation.AgentId : conversation.UserId;

            await Clients.User(otherUserId).SendAsync("ConversationEnded", conversationId);
            await Clients.Caller.SendAsync("ConversationEnded", conversationId);
        }

        private async Task<string> GetAvailableAgent()
        {
            // Get all online agents
            var onlineAgents = _connections.Values
                .Where(c => c.IsAgent)
                .Select(c => c.UserId)
                .Distinct()
                .ToList();

            if (!onlineAgents.Any()) return null;

            // Get agent with least active conversations
            var agentLoads = await _chatService.GetAgentLoads(onlineAgents);

            // Filter agents who haven't reached max capacity
            var availableAgents = new List<string>();
            foreach (var load in agentLoads)
            {
                var maxChats = await _userService.GetAgentMaxChats(load.AgentId);
                if (load.ActiveChats < maxChats)
                {
                    availableAgents.Add(load.AgentId);
                }
            }

            if (!availableAgents.Any()) return null;

            // Return agent with lowest load
            return agentLoads
                .Where(a => availableAgents.Contains(a.AgentId))
                .OrderBy(a => a.ActiveChats)
                .FirstOrDefault()?.AgentId;
        }

        private async Task ReassignUsersFromAgent(string agentId)
        {
            var activeConversations = await _chatService.GetActiveConversationsByAgent(agentId);

            foreach (var conv in activeConversations)
            {
                await Clients.User(conv.UserId).SendAsync("AgentDisconnected");
                var userName = await _userService.GetUserName(conv.UserId);
                var userEmail = await _userService.GetUserEmail(conv.UserId);
                var userConnection = _connections.Values.FirstOrDefault(c => c.UserId == conv.UserId);

                if (userConnection != null)
                {
                    _userQueue[conv.UserId] = new WaitingUserInfo
                    {
                        UserId = conv.UserId,
                        UserName = userName,
                        Email = userEmail,
                        ConnectionId = userConnection.ConnectionId,
                        WaitingSince = DateTime.UtcNow
                    };
                }
            }

            await SyncQueueToAllAgents();
        }

        private async Task AssignAgentToUser(string userId, string email, string connectionId)
        {
            var availableAgent = await GetAvailableAgent();

            if (availableAgent != null)
            {
                var userName = await _userService.GetUserName(userId);

                // Notify agent of new user
                await Clients.User(availableAgent).SendAsync("NewUserWaiting", new WaitingUserDTO 
                {
                    UserId = userId,
                    UserName = userName,
                    UserEmail = email,
                    WaitingSince = DateTime.UtcNow
                });
            }
            else
            {
                var userName = await _userService.GetUserName(userId);

                _userQueue[userId] = new WaitingUserInfo
                {
                    UserId = userId,
                    UserName = userName,
                    Email = email,
                    ConnectionId = connectionId,
                    WaitingSince = DateTime.UtcNow
                };

                // Calculate queue position
                var queuePosition = _userQueue.Keys.ToList().IndexOf(userId) + 1;
                await Clients.Caller.SendAsync("AddedToQueue", queuePosition);

                // Sync queue to all agents
                await SyncQueueToAllAgents();
            }
        }

        private async Task SyncQueueToAllAgents()
        {
            var waitingUsers = _userQueue.Values
                .Select(w => new
                {
                    userId = w.UserId,
                    userName = w.UserName,
                    userEmail = w.Email,
                    waitingSince = w.WaitingSince
                })
                .OrderBy(w => w.waitingSince) // Oldest first
                .ToList();

            await Clients.Group("Agents").SendAsync("QueueSync", new
            {
                users = waitingUsers,
                count = waitingUsers.Count,
                timestamp = DateTime.UtcNow
            });
        }
    }

    public class UserConnection
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string ConnectionId { get; set; }
        public bool IsAgent { get; set; }
        public bool IsOnline { get; set; }
        public DateTime ConnectedAt { get; set; }
    }

    public class AgentLoad
    {
        public string AgentId { get; set; }
        public int ActiveChats { get; set; }
    }

    public class WaitingUserInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ConnectionId { get; set; }
        public DateTime WaitingSince { get; set; }
    }
}
