using DREAMHOMES.Models;
using DREAMHOMES.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DREAMHOMESTEST.ServicesTest
{
    [TestFixture]
    public class UserServiceTest
    {
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private UserService _userService;

        [SetUp]
        public void SetUp()
        {
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<ApplicationUser>>().Object,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new IdentityErrorDescriber(),
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object
            );

            _userService = new UserService(_mockUserManager.Object);
        }

        [Test]
        public async Task UpdateUserStatus_WithValidUserId_ShouldSetIsOnlineTrue()
        {
            // ARRANGE
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, UserName = "testuser", Email = "test@example.com", IsOnline = false };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

            // ACT
            await _userService.UpdateUserStatus(userId, true);

            // ASSERT
            Assert.That(user.IsOnline, Is.True);
            _mockUserManager.Verify(m => m.UpdateAsync(user), Times.Once);
        }

        [Test]
        public async Task UpdateUserStatus_WithValidUserId_ShouldSetIsOnlineFalse()
        {
            // ARRANGE
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, UserName = "testuser", Email = "test@example.com", IsOnline = true };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

            // ACT
            await _userService.UpdateUserStatus(userId, false);

            // ASSERT
            Assert.That(user.IsOnline, Is.False);
            _mockUserManager.Verify(m => m.UpdateAsync(user), Times.Once);
        }

        [Test]
        public async Task UpdateUserStatus_WithNonExistentUserId_ShouldNotUpdate()
        {
            // ARRANGE
            var userId = "non-existent";

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // ACT
            await _userService.UpdateUserStatus(userId, true);

            // ASSERT
            _mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Test]
        public async Task GetAgentMaxChats_WithValidAgentId_ShouldReturnMaxChats()
        {
            // ARRANGE
            var userId = "agent-123";
            var agent = new ApplicationUser { Id = userId, IsAgent = true, MaxConcurrentChats = 50 };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(agent);

            // ACT
            var result = await _userService.GetAgentMaxChats(userId);

            // ASSERT
            Assert.That(result, Is.EqualTo(50));
        }

        [Test]
        public async Task GetAgentMaxChats_WithNonAgentUser_ShouldReturnZero()
        {
            // ARRANGE
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, IsAgent = false, MaxConcurrentChats = 0 };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

            // ACT
            var result = await _userService.GetAgentMaxChats(userId);

            // ASSERT
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAgentMaxChats_WithNonExistentUserId_ShouldReturnZero()
        {
            // ARRANGE
            var userId = "non-existent";

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // ACT
            var result = await _userService.GetAgentMaxChats(userId);

            // ASSERT
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task GetUserEmail_WithValidUserId_ShouldReturnEmail()
        {
            // ARRANGE
            var userId = "user-123";
            var email = "user@example.com";
            var user = new ApplicationUser { Id = userId, Email = email };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

            // ACT
            var result = await _userService.GetUserEmail(userId);

            // ASSERT
            Assert.That(result, Is.EqualTo(email));
        }

        [Test]
        public async Task GetUserEmail_WithNonExistentUserId_ShouldReturnUnknown()
        {
            // ARRANGE
            var userId = "non-existent";

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // ACT
            var result = await _userService.GetUserEmail(userId);

            // ASSERT
            Assert.That(result, Is.EqualTo("Unknown"));
        }

        [Test]
        public async Task GetUserName_WithValidUsername_ShouldReturnUsername()
        {
            // ARRANGE
            var userId = "user-123";
            var username = "testuser";
            var user = new ApplicationUser { Id = userId, UserName = username, Email = "test@example.com" };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

            // ACT
            var result = await _userService.GetUserName(userId);

            // ASSERT
            Assert.That(result, Is.EqualTo(username));
        }

        [Test]
        public async Task GetUserName_WithNullUsername_ShouldReturnEmail()
        {
            // ARRANGE
            var userId = "user-123";
            var email = "test@example.com";
            var user = new ApplicationUser { Id = userId, UserName = null, Email = email };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

            // ACT
            var result = await _userService.GetUserName(userId);

            // ASSERT
            Assert.That(result, Is.EqualTo(email));
        }

        [Test]
        public async Task GetUserName_WithNonExistentUserId_ShouldReturnUnknown()
        {
            // ARRANGE
            var userId = "non-existent";

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // ACT
            var result = await _userService.GetUserName(userId);

            // ASSERT
            Assert.That(result, Is.EqualTo("Unknown"));
        }

        [Test]
        public async Task IsAgentOnline_WithOnlineAgent_ShouldReturnTrue()
        {
            // ARRANGE
            var userId = "agent-123";
            var agent = new ApplicationUser { Id = userId, IsAgent = true, IsOnline = true };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(agent);

            // ACT
            var result = await _userService.IsAgentOnline(userId);

            // ASSERT
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsAgentOnline_WithOfflineAgent_ShouldReturnFalse()
        {
            // ARRANGE
            var userId = "agent-123";
            var agent = new ApplicationUser { Id = userId, IsAgent = true, IsOnline = false };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(agent);

            // ACT
            var result = await _userService.IsAgentOnline(userId);

            // ASSERT
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsAgentOnline_WithNonAgent_ShouldReturnFalse()
        {
            // ARRANGE
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, IsAgent = false, IsOnline = true };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

            // ACT
            var result = await _userService.IsAgentOnline(userId);

            // ASSERT
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsAgentOnline_WithNonExistentUserId_ShouldReturnFalse()
        {
            // ARRANGE
            var userId = "non-existent";

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // ACT
            var result = await _userService.IsAgentOnline(userId);

            // ASSERT
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetUserByEmail_WithValidEmail_ShouldReturnUser()
        {
            // ARRANGE
            var email = "user@example.com";
            var user = new ApplicationUser { Id = "user-123", Email = email, UserName = "testuser" };

            _mockUserManager.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync(user);

            // ACT
            var result = await _userService.GetUserByEmail(email);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo(email));
            _mockUserManager.Verify(m => m.FindByEmailAsync(email), Times.Once);
        }

        [Test]
        public async Task GetUserByEmail_WithNonExistentEmail_ShouldReturnNull()
        {
            // ARRANGE
            var email = "nonexistent@example.com";

            _mockUserManager.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync((ApplicationUser)null);

            // ACT
            var result = await _userService.GetUserByEmail(email);

            // ASSERT
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateUserWithRole_WithValidEmailAndPasswordAsUser_ShouldCreateUserWithUserRole()
        {
            // ARRANGE
            var email = "newuser@example.com";
            var password = "Password@123";
            var isAgent = false;

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            // ACT
            var result = await _userService.CreateUserWithRole(email, password, isAgent);

            // ASSERT
            Assert.That(result.Succeeded, Is.True);
            _mockUserManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password), Times.Once);
            _mockUserManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
        }

        [Test]
        public async Task CreateUserWithRole_WithValidEmailAndPasswordAsAgent_ShouldCreateAgentWithAgentRole()
        {
            // ARRANGE
            var email = "newagent@example.com";
            var password = "Password@123";
            var isAgent = true;

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Agent"))
                .ReturnsAsync(IdentityResult.Success);

            // ACT
            var result = await _userService.CreateUserWithRole(email, password, isAgent);

            // ASSERT
            Assert.That(result.Succeeded, Is.True);
            _mockUserManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password), Times.Once);
            _mockUserManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Agent"), Times.Once);
        }

        [Test]
        public async Task CreateUserWithRole_WithAgentFlag_ShouldSetIsAgentTrueAndMaxChats()
        {
            // ARRANGE
            var email = "agent@example.com";
            var password = "Password@123";
            ApplicationUser createdUser = null;

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password))
                .Callback<ApplicationUser, string>((user, pwd) => createdUser = user)
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Agent"))
                .ReturnsAsync(IdentityResult.Success);

            // ACT
            await _userService.CreateUserWithRole(email, password, true);

            // ASSERT
            Assert.That(createdUser, Is.Not.Null);
            Assert.That(createdUser.IsAgent, Is.True);
            Assert.That(createdUser.MaxConcurrentChats, Is.EqualTo(50));
        }

        [Test]
        public async Task CreateUserWithRole_WithCreateFailure_ShouldNotAddRole()
        {
            // ARRANGE
            var email = "newuser@example.com";
            var password = "Password@123";
            var errors = new[] { new IdentityError { Description = "Creation failed" } };

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // ACT
            var result = await _userService.CreateUserWithRole(email, password, false);

            // ASSERT
            Assert.That(result.Succeeded, Is.False);
            _mockUserManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ResetPassword_WithValidEmail_ShouldResetPassword()
        {
            // ARRANGE
            var email = "user@example.com";
            var newPassword = "NewPassword@123";
            var user = new ApplicationUser { Id = "user-123", Email = email, UserName = "testuser" };

            _mockUserManager.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
            _mockUserManager.Setup(m => m.ResetPasswordAsync(user, "reset-token", newPassword))
                .ReturnsAsync(IdentityResult.Success);

            // ACT
            var result = await _userService.ResetPassword(email, newPassword);

            // ASSERT
            Assert.That(result.Succeeded, Is.True);
            _mockUserManager.Verify(m => m.GeneratePasswordResetTokenAsync(user), Times.Once);
            _mockUserManager.Verify(m => m.ResetPasswordAsync(user, "reset-token", newPassword), Times.Once);
        }

        [Test]
        public async Task ResetPassword_WithNonExistentEmail_ShouldReturnFailure()
        {
            // ARRANGE
            var email = "nonexistent@example.com";
            var newPassword = "NewPassword@123";

            _mockUserManager.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync((ApplicationUser)null);

            // ACT
            var result = await _userService.ResetPassword(email, newPassword);

            // ASSERT
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors.First().Description, Does.Contain("User not found"));
        }

        [Test]
        public async Task ResetPassword_WhenResetFails_ShouldReturnFailure()
        {
            // ARRANGE
            var email = "user@example.com";
            var newPassword = "NewPassword@123";
            var user = new ApplicationUser { Id = "user-123", Email = email, UserName = "testuser" };
            var errors = new[] { new IdentityError { Description = "Reset failed" } };

            _mockUserManager.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
            _mockUserManager.Setup(m => m.ResetPasswordAsync(user, "reset-token", newPassword))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // ACT
            var result = await _userService.ResetPassword(email, newPassword);

            // ASSERT
            Assert.That(result.Succeeded, Is.False);
        }
    }
}
