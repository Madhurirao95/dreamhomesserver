using DREAMHOMES.Configuration;
using DREAMHOMES.Models;
using DREAMHOMES.Services;
using DREAMHOMES.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DREAMHOMESTEST.ServicesTest
{
    [TestFixture]
    public class AuthenticationServiceTest
    {
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<IUserService> _mockUserService;
        private Mock<IOptions<JwtBearerTokenSettings>> _mockJwtOptions;
        private AuthenticationService _authenticationService;
        private JwtBearerTokenSettings _jwtBearerTokenSettings;
        private PasswordHasher<ApplicationUser> _passwordHasher;

        [SetUp]
        public void SetUp()
        {
            _passwordHasher = new PasswordHasher<ApplicationUser>();

            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                _passwordHasher,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new IdentityErrorDescriber(),
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object
            );

            _mockUserService = new Mock<IUserService>();

            _jwtBearerTokenSettings = new JwtBearerTokenSettings
            {
                SecretKey = "this_is_a_very_long_secret_key_that_is_at_least_32_characters_long_for_testing",
                Audience = "https://dreamhomes-7hqb.vercel.app",
                Issuer = "https://dreamhomes-api.com",
                ExpiryTimeInMinutes = 1440
            };

            _mockJwtOptions = new Mock<IOptions<JwtBearerTokenSettings>>();
            _mockJwtOptions.Setup(x => x.Value).Returns(_jwtBearerTokenSettings);

            _authenticationService = new AuthenticationService(
                _mockUserManager.Object,
                _mockUserService.Object,
                _mockJwtOptions.Object
            );
        }

        [Test]
        public async Task CreateAccount_WithValidEmailAndPassword_ShouldCreateUserWithUserRole()
        {
            var email = "user@example.com";
            var password = "Password@123";
            var isAgent = false;

            _mockUserService
                .Setup(u => u.CreateUserWithRole(email, password, isAgent))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _authenticationService.CreateAccount(email, password, isAgent);

            Assert.That(result.Succeeded, Is.True);
            _mockUserService.Verify(u => u.CreateUserWithRole(email, password, isAgent), Times.Once);
        }

        [Test]
        public async Task CreateAccount_WithValidEmailAndPasswordAsAgent_ShouldCreateUserWithAgentRole()
        {
            var email = "agent@example.com";
            var password = "Password@123";
            var isAgent = true;

            _mockUserService
                .Setup(u => u.CreateUserWithRole(email, password, isAgent))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _authenticationService.CreateAccount(email, password, isAgent);

            Assert.That(result.Succeeded, Is.True);
            _mockUserService.Verify(u => u.CreateUserWithRole(email, password, true), Times.Once);
        }

        [Test]
        public async Task CreateAccount_WithInvalidEmail_ShouldReturnFailedResult()
        {
            var email = "invalid-email";
            var password = "Password@123";
            var errors = new[] { new IdentityError { Code = "InvalidEmail", Description = "Email is not valid" } };

            _mockUserService
                .Setup(u => u.CreateUserWithRole(email, password, false))
                .ReturnsAsync(IdentityResult.Failed(errors));

            var result = await _authenticationService.CreateAccount(email, password);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors, Is.Not.Empty);
        }

        [Test]
        public async Task CreateAccount_WithExistingEmail_ShouldReturnFailedResult()
        {
            var email = "existing@example.com";
            var password = "Password@123";
            var errors = new[] { new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" } };

            _mockUserService
                .Setup(u => u.CreateUserWithRole(email, password, false))
                .ReturnsAsync(IdentityResult.Failed(errors));

            var result = await _authenticationService.CreateAccount(email, password);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors.First().Code, Is.EqualTo("DuplicateEmail"));
        }

        [Test]
        public async Task IsAnExistingUser_WithExistingEmail_ShouldReturnTrue()
        {
            var email = "existing@example.com";
            var user = new ApplicationUser { Email = email, UserName = email };

            _mockUserService
                .Setup(u => u.GetUserByEmail(email))
                .ReturnsAsync(user);

            var result = await _authenticationService.IsAnExistingUser(email);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsAnExistingUser_WithNonExistingEmail_ShouldReturnFalse()
        {
            var email = "nonexisting@example.com";

            _mockUserService
                .Setup(u => u.GetUserByEmail(email))
                .ReturnsAsync((ApplicationUser)null);

            var result = await _authenticationService.IsAnExistingUser(email);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsPasswordValid_WithCorrectPassword_ShouldReturnTrue()
        {
            var email = "user@example.com";
            var password = "Password@123";
            var user = new ApplicationUser { Email = email, UserName = email, Id = "user-id-123" };

            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            _mockUserService
                .Setup(u => u.GetUserByEmail(email))
                .ReturnsAsync(user);

            var result = await _authenticationService.IsPasswordValid(email, password);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsPasswordValid_WithIncorrectPassword_ShouldReturnFalse()
        {
            var email = "user@example.com";
            var correctPassword = "Password@123";
            var incorrectPassword = "WrongPassword@123";
            var user = new ApplicationUser { Email = email, UserName = email, Id = "user-id-123" };

            user.PasswordHash = _passwordHasher.HashPassword(user, correctPassword);

            _mockUserService
                .Setup(u => u.GetUserByEmail(email))
                .ReturnsAsync(user);

            var result = await _authenticationService.IsPasswordValid(email, incorrectPassword);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsPasswordValid_WithNonExistingUser_ShouldReturnFalse()
        {
            var email = "nonexisting@example.com";
            var password = "Password@123";

            _mockUserService
                .Setup(u => u.GetUserByEmail(email))
                .ReturnsAsync((ApplicationUser)null);

            var result = await _authenticationService.IsPasswordValid(email, password);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsPasswordValid_WithNullPasswordHash_ShouldReturnFalse()
        {
            var email = "user@example.com";
            var password = "Password@123";
            var user = new ApplicationUser { Email = email, UserName = email, Id = "user-id-123", PasswordHash = null };

            _mockUserService
                .Setup(u => u.GetUserByEmail(email))
                .ReturnsAsync(user);

            var result = await _authenticationService.IsPasswordValid(email, password);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task SignIn_WithValidCredentials_ShouldReturnJwtToken()
        {
            var email = "user@example.com";
            var password = "Password@123";
            var user = new ApplicationUser { Id = "user-id-123", Email = email, UserName = email };

            _mockUserService
                .Setup(u => u.GetUserByEmail(email))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            var result = await _authenticationService.SignIn(email, password);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<string>());
            var token = result as string;
            Assert.That(token, Is.Not.Empty);
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            Assert.That(jwtToken, Is.Not.Null);
            Assert.That(jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value, Is.EqualTo(email));
        }

        [Test]
        public async Task SignIn_WithNonExistingUser_ShouldThrowException()
        {
            var email = "nonexisting@example.com";
            var password = "Password@123";

            _mockUserService
                .Setup(u => u.GetUserByEmail(email))
                .ReturnsAsync((ApplicationUser)null);

            Assert.ThrowsAsync<NullReferenceException>(
                async () => await _authenticationService.SignIn(email, password));
        }

        [Test]
        public async Task SignIn_UserWithMultipleRoles_ShouldIncludeAllRolesInToken()
        {
            var email = "admin@example.com";
            var password = "Password@123";
            var user = new ApplicationUser { Id = "user-id-123", Email = email, UserName = email };
            var roles = new List<string> { "Admin", "Agent", "User" };

            _mockUserService
                .Setup(u => u.GetUserByEmail(email))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(roles);

            var result = await _authenticationService.SignIn(email, password);

            Assert.That(result, Is.Not.Null);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(result as string) as JwtSecurityToken;
            
            var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            Assert.That(roleClaims.Count(), Is.EqualTo(3));
        }

        [Test]
        public void IsAgent_ShouldThrowNotImplementedException()
        {
            var email = "agent@example.com";

            Assert.ThrowsAsync<NotImplementedException>(
                async () => await _authenticationService.IsAgent(email));
        }

        [Test]
        public async Task GenerateToken_WithUserWithoutRoles_ShouldCreateValidTokenWithBasicClaims()
        {
            var email = "user@example.com";
            var user = new ApplicationUser { Id = "user-id-123", Email = email, UserName = email };

            _mockUserService
                .Setup(u => u.GetUserByEmail(email))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            var result = await _authenticationService.SignIn(email, email);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(result as string) as JwtSecurityToken;
            
            Assert.That(jwtToken.Claims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "user-id-123"), Is.True);
            Assert.That(jwtToken.Claims.Any(c => c.Type == ClaimTypes.Email && c.Value == email), Is.True);
            Assert.That(jwtToken.Issuer, Is.EqualTo(_jwtBearerTokenSettings.Issuer));
            Assert.That(jwtToken.Audiences.First(), Is.EqualTo(_jwtBearerTokenSettings.Audience));
        }

        [Test]
        public async Task GenerateToken_TokenShouldHaveCorrectExpiry()
        {
            var email = "user@example.com";
            var user = new ApplicationUser { Id = "user-id-123", Email = email, UserName = email };

            _mockUserService
                .Setup(u => u.GetUserByEmail(email))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            var beforeGeneration = DateTime.UtcNow;

            var result = await _authenticationService.SignIn(email, email);

            var afterGeneration = DateTime.UtcNow;

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(result as string) as JwtSecurityToken;
            
            var expectedExpiry = _jwtBearerTokenSettings.ExpiryTimeInMinutes;
            var actualMinutesDifference = (jwtToken.ValidTo - beforeGeneration).TotalMinutes;
            
            Assert.That(actualMinutesDifference, Is.GreaterThan(expectedExpiry - 1));
            Assert.That(actualMinutesDifference, Is.LessThan(expectedExpiry + 1));
        }
    }
}
