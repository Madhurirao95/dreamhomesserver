using AutoMapper;
using DREAMHOMES.Hubs;
using DREAMHOMES.Models;
using DREAMHOMES.Models.Repository.Interfaces;
using DREAMHOMES.Services;
using Moq;

namespace DREAMHOMESTEST.ServicesTest
{
    [TestFixture]
    public class ChatServiceTest
    {
        private Mock<IChatMessageRepository> _mockMessageRepository;
        private Mock<IConversationRepository> _mockConversationRepository;
        private ChatService _chatService;

        [SetUp]
        public void SetUp()
        {
            _mockMessageRepository = new Mock<IChatMessageRepository>();
            _mockConversationRepository = new Mock<IConversationRepository>();

            _chatService = new ChatService(
                _mockMessageRepository.Object,
                _mockConversationRepository.Object
            );
        }

        [Test]
        public async Task CreateConversation_WithValidUserIdAndAgentId_ShouldReturnConversation()
        {
            // ARRANGE
            var userId = "user-123";
            var agentId = "agent-456";
            var conversation = new Conversation { Id = "conv-789", UserId = userId, AgentId = agentId };

            _mockConversationRepository
                .Setup(r => r.SaveConversation(It.IsAny<Conversation>()))
                .ReturnsAsync(conversation);

            // ACT
            var result = await _chatService.CreateConversation(userId, agentId);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.AgentId, Is.EqualTo(agentId));
            _mockConversationRepository.Verify(r => r.SaveConversation(It.IsAny<Conversation>()), Times.Once);
        }

        [Test]
        public void CreateConversation_WhenRepositoryThrows_ShouldPropagateException()
        {
            // ARRANGE
            var userId = "user-123";
            var agentId = "agent-456";

            _mockConversationRepository
                .Setup(r => r.SaveConversation(It.IsAny<Conversation>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _chatService.CreateConversation(userId, agentId));
        }

        [Test]
        public async Task CreateMessage_WithValidParameters_ShouldReturnChatMessage()
        {
            // ARRANGE
            var userId = "user-123";
            var conversationId = "conv-789";
            var messageText = "Hello, how can I help?";
            var timestamp = DateTime.UtcNow;
            var isFromAgent = true;
            var chatMessage = new ChatMessage 
            { 
                Id = "msg-001",
                Content = messageText, 
                ConversationId = conversationId, 
                UserId = userId, 
                IsFromAgent = isFromAgent, 
                Timestamp = timestamp 
            };

            _mockMessageRepository
                .Setup(r => r.SaveMessage(It.IsAny<ChatMessage>()))
                .ReturnsAsync(chatMessage);

            // ACT
            var result = await _chatService.CreateMessage(userId, conversationId, messageText, timestamp, isFromAgent);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.EqualTo(messageText));
            Assert.That(result.ConversationId, Is.EqualTo(conversationId));
            Assert.That(result.IsFromAgent, Is.EqualTo(isFromAgent));
            _mockMessageRepository.Verify(r => r.SaveMessage(It.IsAny<ChatMessage>()), Times.Once);
        }

        [Test]
        public async Task CreateMessage_FromUser_ShouldSetIsFromAgentFalse()
        {
            // ARRANGE
            var userId = "user-123";
            var conversationId = "conv-789";
            var messageText = "Hi there";
            var timestamp = DateTime.UtcNow;
            var isFromAgent = false;
            var chatMessage = new ChatMessage 
            { 
                Content = messageText, 
                ConversationId = conversationId, 
                UserId = userId, 
                IsFromAgent = isFromAgent, 
                Timestamp = timestamp 
            };

            _mockMessageRepository
                .Setup(r => r.SaveMessage(It.IsAny<ChatMessage>()))
                .ReturnsAsync(chatMessage);

            // ACT
            var result = await _chatService.CreateMessage(userId, conversationId, messageText, timestamp, isFromAgent);

            // ASSERT
            Assert.That(result.IsFromAgent, Is.False);
        }

        [Test]
        public void CreateMessage_WhenRepositoryThrows_ShouldPropagateException()
        {
            // ARRANGE
            _mockMessageRepository
                .Setup(r => r.SaveMessage(It.IsAny<ChatMessage>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _chatService.CreateMessage("user-123", "conv-789", "Hello", DateTime.UtcNow, true));
        }

        [Test]
        public async Task EndConversation_WithValidConversationId_ShouldCallRepository()
        {
            // ARRANGE
            var conversationId = "conv-789";

            _mockConversationRepository
                .Setup(r => r.EndConversation(conversationId))
                .Returns(Task.CompletedTask);

            // ACT
            await _chatService.EndConversation(conversationId);

            // ASSERT
            _mockConversationRepository.Verify(r => r.EndConversation(conversationId), Times.Once);
        }

        [Test]
        public void EndConversation_WhenRepositoryThrows_ShouldPropagateException()
        {
            // ARRANGE
            var conversationId = "conv-789";

            _mockConversationRepository
                .Setup(r => r.EndConversation(conversationId))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _chatService.EndConversation(conversationId));
        }

        [Test]
        public async Task GetActiveConversationsByAgent_WithValidAgentId_ShouldReturnConversations()
        {
            // ARRANGE
            var agentId = "agent-456";
            var conversations = new List<Conversation>
            {
                new Conversation { Id = "conv-1", AgentId = agentId },
                new Conversation { Id = "conv-2", AgentId = agentId }
            };

            _mockConversationRepository
                .Setup(r => r.GetActiveConversationsByAgent(agentId))
                .ReturnsAsync(conversations);

            // ACT
            var result = await _chatService.GetActiveConversationsByAgent(agentId);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            _mockConversationRepository.Verify(r => r.GetActiveConversationsByAgent(agentId), Times.Once);
        }

        [Test]
        public async Task GetActiveConversationsByAgent_WithNoConversations_ShouldReturnEmptyList()
        {
            // ARRANGE
            var agentId = "agent-456";

            _mockConversationRepository
                .Setup(r => r.GetActiveConversationsByAgent(agentId))
                .ReturnsAsync(new List<Conversation>());

            // ACT
            var result = await _chatService.GetActiveConversationsByAgent(agentId);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetActiveConversationsByAgent_WhenRepositoryThrows_ShouldPropagateException()
        {
            // ARRANGE
            var agentId = "agent-456";

            _mockConversationRepository
                .Setup(r => r.GetActiveConversationsByAgent(agentId))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _chatService.GetActiveConversationsByAgent(agentId));
        }

        [Test]
        public async Task GetAgentCurrentLoad_WithValidAgentId_ShouldReturnLoadCount()
        {
            // ARRANGE
            var agentId = "agent-456";
            var loadCount = 5;

            _mockConversationRepository
                .Setup(r => r.GetAgentCurrentLoad(agentId))
                .ReturnsAsync(loadCount);

            // ACT
            var result = await _chatService.GetAgentCurrentLoad(agentId);

            // ASSERT
            Assert.That(result, Is.EqualTo(loadCount));
            _mockConversationRepository.Verify(r => r.GetAgentCurrentLoad(agentId), Times.Once);
        }

        [Test]
        public async Task GetAgentCurrentLoad_WithZeroLoad_ShouldReturnZero()
        {
            // ARRANGE
            var agentId = "agent-456";

            _mockConversationRepository
                .Setup(r => r.GetAgentCurrentLoad(agentId))
                .ReturnsAsync(0);

            // ACT
            var result = await _chatService.GetAgentCurrentLoad(agentId);

            // ASSERT
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetAgentCurrentLoad_WhenRepositoryThrows_ShouldPropagateException()
        {
            // ARRANGE
            var agentId = "agent-456";

            _mockConversationRepository
                .Setup(r => r.GetAgentCurrentLoad(agentId))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _chatService.GetAgentCurrentLoad(agentId));
        }

        [Test]
        public async Task GetAgentLoads_WithValidAgentIds_ShouldReturnAgentLoads()
        {
            // ARRANGE
            var agentIds = new List<string> { "agent-1", "agent-2", "agent-3" };
            var agentLoads = new List<AgentLoad>
            {
                new AgentLoad { AgentId = "agent-1", ActiveChats = 5 },
                new AgentLoad { AgentId = "agent-2", ActiveChats = 3 },
                new AgentLoad { AgentId = "agent-3", ActiveChats = 0 }
            };

            _mockConversationRepository
                .Setup(r => r.GetAgentLoads(agentIds))
                .ReturnsAsync(agentLoads);

            // ACT
            var result = await _chatService.GetAgentLoads(agentIds);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].ActiveChats, Is.EqualTo(5));
            _mockConversationRepository.Verify(r => r.GetAgentLoads(agentIds), Times.Once);
        }

        [Test]
        public async Task GetAgentLoads_WithEmptyAgentIdList_ShouldReturnEmptyList()
        {
            // ARRANGE
            var agentIds = new List<string>();

            _mockConversationRepository
                .Setup(r => r.GetAgentLoads(agentIds))
                .ReturnsAsync(new List<AgentLoad>());

            // ACT
            var result = await _chatService.GetAgentLoads(agentIds);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAgentLoads_WhenRepositoryThrows_ShouldPropagateException()
        {
            // ARRANGE
            var agentIds = new List<string> { "agent-1" };

            _mockConversationRepository
                .Setup(r => r.GetAgentLoads(agentIds))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _chatService.GetAgentLoads(agentIds));
        }

        [Test]
        public async Task GetConversation_WithValidConversationId_ShouldReturnConversation()
        {
            // ARRANGE
            var conversationId = "conv-789";
            var conversation = new Conversation { Id = conversationId, UserId = "user-123", AgentId = "agent-456" };

            _mockConversationRepository
                .Setup(r => r.Get(conversationId))
                .ReturnsAsync(conversation);

            // ACT
            var result = await _chatService.GetConversation(conversationId);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(conversationId));
            _mockConversationRepository.Verify(r => r.Get(conversationId), Times.Once);
        }

        [Test]
        public async Task GetConversation_WithNonExistentConversationId_ShouldReturnNull()
        {
            // ARRANGE
            var conversationId = "non-existent";

            _mockConversationRepository
                .Setup(r => r.Get(conversationId))
                .ReturnsAsync((Conversation)null);

            // ACT
            var result = await _chatService.GetConversation(conversationId);

            // ASSERT
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetConversation_WhenRepositoryThrows_ShouldPropagateException()
        {
            // ARRANGE
            var conversationId = "conv-789";

            _mockConversationRepository
                .Setup(r => r.Get(conversationId))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _chatService.GetConversation(conversationId));
        }

        [Test]
        public async Task GetConversationMessages_WithValidConversationId_ShouldReturnMessages()
        {
            // ARRANGE
            var conversationId = "conv-789";
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Id = "msg-1", Content = "Hello", ConversationId = conversationId },
                new ChatMessage { Id = "msg-2", Content = "Hi there", ConversationId = conversationId }
            };

            _mockConversationRepository
                .Setup(r => r.GetConversationMessages(conversationId))
                .ReturnsAsync(messages);

            // ACT
            var result = await _chatService.GetConversationMessages(conversationId);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            _mockConversationRepository.Verify(r => r.GetConversationMessages(conversationId), Times.Once);
        }

        [Test]
        public async Task GetConversationMessages_WithNoMessages_ShouldReturnEmptyList()
        {
            // ARRANGE
            var conversationId = "conv-789";

            _mockConversationRepository
                .Setup(r => r.GetConversationMessages(conversationId))
                .ReturnsAsync(new List<ChatMessage>());

            // ACT
            var result = await _chatService.GetConversationMessages(conversationId);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetConversationMessages_WhenRepositoryThrows_ShouldPropagateException()
        {
            // ARRANGE
            var conversationId = "conv-789";

            _mockConversationRepository
                .Setup(r => r.GetConversationMessages(conversationId))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _chatService.GetConversationMessages(conversationId));
        }

        [Test]
        public async Task GetUserConversationHistory_WithValidUserId_ShouldReturnConversations()
        {
            // ARRANGE
            var userId = "user-123";
            var conversations = new List<Conversation>
            {
                new Conversation { Id = "conv-1", UserId = userId, AgentId = "agent-1" },
                new Conversation { Id = "conv-2", UserId = userId, AgentId = "agent-2" },
                new Conversation { Id = "conv-3", UserId = userId, AgentId = "agent-3" }
            };

            _mockConversationRepository
                .Setup(r => r.GetUserConversationHistory(userId))
                .ReturnsAsync(conversations);

            // ACT
            var result = await _chatService.GetUserConversationHistory(userId);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
            _mockConversationRepository.Verify(r => r.GetUserConversationHistory(userId), Times.Once);
        }

        [Test]
        public async Task GetUserConversationHistory_WithNoConversations_ShouldReturnEmptyList()
        {
            // ARRANGE
            var userId = "user-123";

            _mockConversationRepository
                .Setup(r => r.GetUserConversationHistory(userId))
                .ReturnsAsync(new List<Conversation>());

            // ACT
            var result = await _chatService.GetUserConversationHistory(userId);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetUserConversationHistory_WhenRepositoryThrows_ShouldPropagateException()
        {
            // ARRANGE
            var userId = "user-123";

            _mockConversationRepository
                .Setup(r => r.GetUserConversationHistory(userId))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _chatService.GetUserConversationHistory(userId));
        }
    }
}
