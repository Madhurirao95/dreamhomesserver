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
        private readonly ILogger<ChatHub> _logger;
        private static ConcurrentDictionary<string, UserConnection> _connections = new();
        private static ConcurrentDictionary<string, WaitingUserInfo> _userQueue = new();

        public ChatHub(IChatService chatService, IUserService userService, IMapper mapper, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            // Get user info from JWT claims
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            var connectionId = Context.ConnectionId;

            // Check if user is an Agent (from JWT role claim)
            var isAgent = Context.User?.IsInRole("Agent") ?? false;

            _logger.LogInformation(
                "[OnConnected] ConnectionId={ConnectionId} UserId={UserId} Email={Email} IsAgent={IsAgent}",
                connectionId, userId ?? "NULL", email ?? "NULL", isAgent);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[OnConnected] Aborting connection — userId claim is missing. ConnectionId={ConnectionId}", connectionId);
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

            _logger.LogInformation(
                "[OnConnected] Connection registered. TotalConnections={Total} TotalQueue={Queue}",
                _connections.Count, _userQueue.Count);

            if (isAgent)
            {
                // Agent connected
                await Groups.AddToGroupAsync(connectionId, "Agents");
                _logger.LogInformation("[OnConnected] AddToGroupAsync completed for Agents. UserId={UserId}", userId);

                await this._userService.UpdateUserStatus(userId, true);

                await Clients.Caller.SendAsync("AgentConnected", new
                {
                    userId,
                    email
                });
                _logger.LogInformation("[OnConnected] Sent AgentConnected to caller. UserId={UserId}", userId);

                // Send initial queue directly to caller via connectionId — SyncQueueToAllAgents uses
                // Clients.Group("Agents") which may not reflect this agent's membership yet on Azure
                // due to group registration latency. Clients.Client(connectionId) bypasses this entirely.
                await SyncQueueToAllAgents(connectionId);
                _logger.LogInformation(
                    "[OnConnected] Initial QueueSync sent directly to caller. UserId={UserId} QueueCount={Count}",
                    userId, _userQueue.Count);
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

            if (exception != null)
                _logger.LogError(exception, "[OnDisconnected] ConnectionId={ConnectionId} disconnected with error", connectionId);
            else
                _logger.LogInformation("[OnDisconnected] ConnectionId={ConnectionId} disconnected cleanly", connectionId);

            if (_connections.TryRemove(connectionId, out var connection))
            {
                _logger.LogInformation(
                    "[OnDisconnected] Removed connection. UserId={UserId} IsAgent={IsAgent} TotalConnections={Total}",
                    connection.UserId, connection.IsAgent, _connections.Count);

                if (connection.IsAgent)
                {
                    await Groups.RemoveFromGroupAsync(connectionId, "Agents");
                    await _userService.UpdateUserStatus(connection.UserId, false);
                    _logger.LogInformation("[OnDisconnected] Agent removed from group and status set offline. UserId={UserId}", connection.UserId);
                    await ReassignUsersFromAgent(connection.UserId);
                }
                else
                {
                    _userQueue.TryRemove(connection.UserId, out _);
                    _logger.LogInformation(
                        "[OnDisconnected] User removed from queue. UserId={UserId} TotalQueue={Queue}",
                        connection.UserId, _userQueue.Count);
                }
            }
            else
            {
                _logger.LogWarning("[OnDisconnected] ConnectionId={ConnectionId} not found in _connections dictionary", connectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string conversationId, string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAgent = Context.User?.IsInRole("Agent") ?? false;

            _logger.LogInformation(
                "[SendMessage] UserId={UserId} IsAgent={IsAgent} ConversationId={ConversationId} MessageLength={Length}",
                userId ?? "NULL", isAgent, conversationId, message?.Length ?? 0);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[SendMessage] Unauthorized — userId claim missing. ConversationId={ConversationId}", conversationId);
                await Clients.Caller.SendAsync("Error", "Unauthorized");
                return;
            }

            // Verify user is part of this conversation
            var conversation = await _chatService.GetConversation(conversationId);
            if (conversation == null ||
                (conversation.UserId != userId && conversation.AgentId != userId))
            {
                _logger.LogWarning(
                    "[SendMessage] Not authorized for conversation. UserId={UserId} ConversationId={ConversationId}",
                    userId, conversationId);
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

            _logger.LogInformation("[SendMessage] Message saved. MessageId={MessageId}", savedMessage?.Id ?? "NULL");

            // Get recipient
            var recipientId = isAgent ? conversation.UserId : conversation.AgentId;

            _logger.LogInformation("[SendMessage] Sending ReceiveMessage to recipient. RecipientId={RecipientId}", recipientId);

            // Send to the other party
            await Clients.User(recipientId).SendAsync("ReceiveMessage", this._mapper.Map<ChatMessageDTO>(savedMessage));

            // Confirm to sender
            await Clients.Caller.SendAsync("MessageSent", this._mapper.Map<ChatMessageDTO>(savedMessage));

            _logger.LogInformation("[SendMessage] Message delivered. ConversationId={ConversationId}", conversationId);
        }

        [Authorize(Roles = "Agent")] // Only agents can accept chats
        public async Task AgentAcceptChat(string waitingUserId)
        {
            var agentId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var agentEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

            _logger.LogInformation("[AgentAcceptChat] AgentId={AgentId} WaitingUserId={WaitingUserId}", agentId ?? "NULL", waitingUserId);

            if (string.IsNullOrEmpty(agentId))
            {
                _logger.LogWarning("[AgentAcceptChat] Unauthorized — agentId claim missing");
                await Clients.Caller.SendAsync("Error", "Unauthorized");
                return;
            }

            // Check if agent has reached max concurrent chats
            var agentLoad = await _chatService.GetAgentCurrentLoad(agentId);
            var maxChats = await _userService.GetAgentMaxChats(agentId);

            _logger.LogInformation("[AgentAcceptChat] Load check. AgentId={AgentId} CurrentLoad={Load} MaxChats={Max}", agentId, agentLoad, maxChats);

            if (agentLoad >= maxChats)
            {
                _logger.LogWarning("[AgentAcceptChat] Max chats reached. AgentId={AgentId} Load={Load} Max={Max}", agentId, agentLoad, maxChats);
                await Clients.Caller.SendAsync("Error", "Maximum concurrent chats reached");
                return;
            }

            // Create conversation
            var conversation = await _chatService.CreateConversation(waitingUserId, agentId);
            _logger.LogInformation("[AgentAcceptChat] Conversation created. ConversationId={ConversationId}", conversation?.Id ?? "NULL");

            // Remove user from queue
            _userQueue.TryRemove(waitingUserId, out _);
            _logger.LogInformation("[AgentAcceptChat] User removed from queue. WaitingUserId={WaitingUserId} TotalQueue={Queue}", waitingUserId, _userQueue.Count);

            // Get agent name
            var agentName = await _userService.GetUserName(agentId);

            // Notify user they're connected to an agent
            _logger.LogInformation("[AgentAcceptChat] Sending AgentAssigned to user. WaitingUserId={WaitingUserId}", waitingUserId);
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

            _logger.LogInformation("[AgentAcceptChat] Sending ChatAccepted to agent. AgentId={AgentId}", agentId);
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

            _logger.LogDebug(
                "[SendTypingIndicator] UserId={UserId} IsAgent={IsAgent} ConversationId={ConversationId} IsTyping={IsTyping}",
                userId ?? "NULL", isAgent, conversationId, isTyping);

            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var conversation = await _chatService.GetConversation(conversationId);
            if (conversation == null)
            {
                _logger.LogWarning("[SendTypingIndicator] Conversation not found. ConversationId={ConversationId}", conversationId);
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

            _logger.LogInformation("[EndConversation] UserId={UserId} ConversationId={ConversationId}", userId ?? "NULL", conversationId);

            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var conversation = await _chatService.GetConversation(conversationId);

            // Verify user is part of conversation
            if (conversation == null ||
                (conversation.UserId != userId && conversation.AgentId != userId))
            {
                _logger.LogWarning("[EndConversation] Not authorized. UserId={UserId} ConversationId={ConversationId}", userId, conversationId);
                await Clients.Caller.SendAsync("Error", "Not authorized");
                return;
            }

            await _chatService.EndConversation(conversationId);
            _logger.LogInformation("[EndConversation] Conversation ended in DB. ConversationId={ConversationId}", conversationId);

            var otherUserId = conversation.UserId == userId ? conversation.AgentId : conversation.UserId;

            await Clients.User(otherUserId).SendAsync("ConversationEnded", conversationId);
            await Clients.Caller.SendAsync("ConversationEnded", conversationId);

            _logger.LogInformation("[EndConversation] ConversationEnded sent to both parties. ConversationId={ConversationId}", conversationId);
        }

        private async Task<string> GetAvailableAgent()
        {
            // Get all online agents
            var onlineAgents = _connections.Values
                .Where(c => c.IsAgent)
                .Select(c => c.UserId)
                .Distinct()
                .ToList();

            _logger.LogInformation("[GetAvailableAgent] Online agents in _connections: Count={Count} Agents=[{Agents}]",
                onlineAgents.Count, string.Join(", ", onlineAgents));

            if (!onlineAgents.Any()) return null;

            // Get agent with least active conversations
            var agentLoads = await _chatService.GetAgentLoads(onlineAgents);

            // Filter agents who haven't reached max capacity
            var availableAgents = new List<string>();
            foreach (var load in agentLoads)
            {
                var maxChats = await _userService.GetAgentMaxChats(load.AgentId);
                _logger.LogInformation("[GetAvailableAgent] AgentId={AgentId} ActiveChats={Active} MaxChats={Max}", load.AgentId, load.ActiveChats, maxChats);
                if (load.ActiveChats < maxChats)
                {
                    availableAgents.Add(load.AgentId);
                }
            }

            _logger.LogInformation("[GetAvailableAgent] Agents under capacity: {Count}", availableAgents.Count);

            if (!availableAgents.Any()) return null;

            // Return agent with lowest load
            var selected = agentLoads
                .Where(a => availableAgents.Contains(a.AgentId))
                .OrderBy(a => a.ActiveChats)
                .FirstOrDefault()?.AgentId;

            _logger.LogInformation("[GetAvailableAgent] Selected agent: {AgentId}", selected ?? "NONE");
            return selected;
        }

        private async Task ReassignUsersFromAgent(string agentId)
        {
            _logger.LogInformation("[ReassignUsers] Starting reassignment for disconnected agent. AgentId={AgentId}", agentId);

            var activeConversations = await _chatService.GetActiveConversationsByAgent(agentId);

            _logger.LogInformation("[ReassignUsers] Found {Count} active conversations. AgentId={AgentId}", activeConversations.Count(), agentId);

            foreach (var conv in activeConversations)
            {
                await Clients.User(conv.UserId).SendAsync("AgentDisconnected");
                _logger.LogInformation("[ReassignUsers] Sent AgentDisconnected to user. UserId={UserId}", conv.UserId);

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
                    _logger.LogInformation("[ReassignUsers] User re-added to queue. UserId={UserId} TotalQueue={Queue}", conv.UserId, _userQueue.Count);
                }
                else
                {
                    _logger.LogWarning("[ReassignUsers] User connection not found — cannot re-queue. UserId={UserId}", conv.UserId);
                }
            }

            await SyncQueueToAllAgents();
        }

        private async Task AssignAgentToUser(string userId, string email, string connectionId)
        {
            _logger.LogInformation("[AssignAgentToUser] UserId={UserId} Email={Email}", userId, email ?? "NULL");

            var availableAgent = await GetAvailableAgent();

            if (availableAgent != null)
            {
                var userName = await _userService.GetUserName(userId);

                _logger.LogInformation("[AssignAgentToUser] Agent available — sending NewUserWaiting. AgentId={AgentId} UserId={UserId}", availableAgent, userId);

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
                _logger.LogInformation("[AssignAgentToUser] No agent available — adding to queue. UserId={UserId}", userId);

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

                _logger.LogInformation("[AssignAgentToUser] User queued at position {Position}. UserId={UserId} TotalQueue={Total}", queuePosition, userId, _userQueue.Count);

                await Clients.Caller.SendAsync("AddedToQueue", queuePosition);

                // Sync queue to all agents
                await SyncQueueToAllAgents();
            }
        }

        private async Task SyncQueueToAllAgents(string callerConnectionId = null)
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

            _logger.LogInformation(
                "[SyncQueueToAllAgents] Syncing queue. Count={Count} Target={Target}",
                waitingUsers.Count, callerConnectionId != null ? $"Caller({callerConnectionId})" : "Group(Agents)");

            var payload = new
            {
                users = waitingUsers,
                count = waitingUsers.Count,
                timestamp = DateTime.UtcNow
            };

            // If a specific callerConnectionId is provided, send directly to bypass
            // Azure group registration latency on initial agent connect
            if (callerConnectionId != null)
                await Clients.Client(callerConnectionId).SendAsync("QueueSync", payload);
            else
                await Clients.Group("Agents").SendAsync("QueueSync", payload);
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