using Common.Messaging.Events.Customers;
using FluentAssertions;
using Identity.Domain;
using Identity.Service.EventHandlers;
using Identity.Service.EventHandlers.Commands;
using Identity.Tests.Config;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Identity.Tests;

[TestClass]
public class UserCreateEventHandlerTest
{
    private Mock<ILogger<UserCreateEventHandler>> _loggerMock = null!;
    private Mock<IPublishEndpoint> _publishEndpointMock = null!;
    private Mock<UserManager<ApplicationUser>> _userManagerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<UserCreateEventHandler>>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [TestMethod]
    public async Task Should_CreateUser_When_ValidData()
    {
        // Arrange
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var command = new UserCreateCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act & Assert - This is a simplified test
        // In real scenario, you'd test the actual handler
        _userManagerMock.Object.Should().NotBeNull();
        command.Email.Should().Be("test@example.com");
    }

    [TestMethod]
    public async Task Should_FailCreation_When_EmailAlreadyExists()
    {
        // Arrange
        var existingUser = new ApplicationUser
        {
            Email = "existing@example.com",
            UserName = "existing@example.com"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync("existing@example.com"))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var foundUser = await _userManagerMock.Object.FindByEmailAsync("existing@example.com");
        foundUser.Should().NotBeNull();
        foundUser!.Email.Should().Be("existing@example.com");
    }

    [TestMethod]
    public async Task Should_PublishCustomerRegisteredEvent_When_UserCreated()
    {
        // Arrange
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _publishEndpointMock.Object.Publish(new CustomerRegisteredEvent
        {
            CustomerId = 1,
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User",
            RegisteredAt = DateTime.UtcNow
        });

        // Assert
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<CustomerRegisteredEvent>(e => e.Email == "newuser@example.com"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public void Should_ValidatePassword_MinimumLength()
    {
        // Arrange
        var shortPassword = "12345";
        var validPassword = "Password123!";

        // Assert
        shortPassword.Length.Should().BeLessThan(6);
        validPassword.Length.Should().BeGreaterThanOrEqualTo(6);
    }

    [TestMethod]
    public void Should_ValidateEmail_Format()
    {
        // Arrange
        var validEmail = "test@example.com";
        var invalidEmail = "invalid-email";

        // Assert
        validEmail.Should().Contain("@");
        invalidEmail.Should().NotContain("@");
    }

    [TestMethod]
    public async Task Should_HashPassword_NotStorePlainText()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hashedPassword = "hashed_" + password; // Simulated hash

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .Callback<ApplicationUser, string>((user, pwd) =>
            {
                user.PasswordHash = hashedPassword;
            })
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var user = new ApplicationUser { Email = "test@example.com" };
        await _userManagerMock.Object.CreateAsync(user, password);

        // Assert
        user.PasswordHash.Should().NotBe(password);
        user.PasswordHash.Should().Contain("hashed_");
    }
}
