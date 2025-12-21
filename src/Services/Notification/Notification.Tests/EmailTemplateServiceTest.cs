using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Notification.Api.Services;

namespace Notification.Tests;

/// <summary>
/// Tests para el servicio de templates de email.
/// </summary>
[TestClass]
public class EmailTemplateServiceTest
{
    private Mock<ILogger<EmailTemplateService>> _loggerMock = null!;
    private EmailTemplateService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<EmailTemplateService>>();
        _service = new EmailTemplateService(_loggerMock.Object);
    }

    [TestMethod]
    public async Task Should_RenderEmailConfirmationTemplate()
    {
        // Arrange
        var data = new Dictionary<string, object>
        {
            { "FirstName", "John" },
            { "ConfirmationToken", "ABC123" },
            { "ConfirmationUrl", "https://example.com/confirm" }
        };

        // Act
        var (subject, htmlBody, textBody) = await _service.RenderTemplateAsync("email-confirmation", data);

        // Assert
        subject.Should().Contain("Confirma tu email");
        htmlBody.Should().NotBeNullOrEmpty();
        textBody.Should().Contain("John");
        textBody.Should().Contain("ABC123");
    }

    [TestMethod]
    public async Task Should_RenderPasswordResetTemplate()
    {
        // Arrange
        var data = new Dictionary<string, object>
        {
            { "FirstName", "Jane" },
            { "ResetToken", "RESET456" },
            { "ResetUrl", "https://example.com/reset" }
        };

        // Act
        var (subject, htmlBody, textBody) = await _service.RenderTemplateAsync("password-reset", data);

        // Assert
        subject.Should().Contain("Recuperación de contraseña");
        htmlBody.Should().NotBeNullOrEmpty();
        textBody.Should().Contain("Jane");
        textBody.Should().Contain("RESET456");
    }

    [TestMethod]
    public async Task Should_RenderPasswordChangedTemplate()
    {
        // Arrange
        var data = new Dictionary<string, object>
        {
            { "FirstName", "Bob" }
        };

        // Act
        var (subject, htmlBody, textBody) = await _service.RenderTemplateAsync("password-changed", data);

        // Assert
        subject.Should().Contain("Contraseña actualizada");
        textBody.Should().Contain("Bob");
        textBody.Should().Contain("actualizada");
    }

    [TestMethod]
    public async Task Should_Render2FAEnabledTemplate()
    {
        // Arrange
        var data = new Dictionary<string, object>
        {
            { "FirstName", "Alice" }
        };

        // Act
        var (subject, htmlBody, textBody) = await _service.RenderTemplateAsync("2fa-enabled", data);

        // Assert
        subject.Should().Contain("dos factores");
        textBody.Should().Contain("Alice");
    }

    [TestMethod]
    public async Task Should_RenderDefaultTemplate_WhenTemplateNotFound()
    {
        // Arrange
        var data = new Dictionary<string, object>
        {
            { "Message", "Test message" }
        };

        // Act
        var (subject, htmlBody, textBody) = await _service.RenderTemplateAsync("unknown-template", data);

        // Assert
        subject.Should().Contain("Notificación");
        htmlBody.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public async Task Should_HandleObjectData_WithReflection()
    {
        // Arrange
        var data = new { FirstName = "Test", LastName = "User", Email = "test@example.com" };

        // Act
        var (subject, htmlBody, textBody) = await _service.RenderTemplateAsync("email-confirmation", data);

        // Assert
        subject.Should().NotBeNullOrEmpty();
        textBody.Should().Contain("Test");
    }

    [TestMethod]
    public async Task Should_HandleMissingDataGracefully()
    {
        // Arrange
        var data = new Dictionary<string, object>(); // Empty data

        // Act
        var (subject, htmlBody, textBody) = await _service.RenderTemplateAsync("email-confirmation", data);

        // Assert
        subject.Should().NotBeNullOrEmpty();
        htmlBody.Should().NotBeNullOrEmpty();
    }
}
