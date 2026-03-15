using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Notification.Domain;

namespace Notification.Tests;

/// <summary>
/// Tests unitarios para la lógica de dominio de Notification.
/// </summary>
[TestClass]
public class NotificationDomainTest
{
    private Domain.Notification CreateNotification(
        bool isRead = false,
        DateTime? expiresAt = null)
    {
        return new Domain.Notification
        {
            NotificationId = 1,
            UserId = 100,
            Type = NotificationType.OrderStatus,
            Title = "Test Notification",
            Message = "This is a test notification message",
            Data = "{\"orderId\": 123}",
            IsRead = isRead,
            ReadAt = isRead ? DateTime.UtcNow : null,
            Priority = NotificationPriority.Normal,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    #region Computed Properties Tests

    [TestMethod]
    public void IsExpired_Should_ReturnFalse_WhenNoExpirationSet()
    {
        // Arrange
        var notification = CreateNotification(expiresAt: null);

        // Assert
        notification.IsExpired.Should().BeFalse();
    }

    [TestMethod]
    public void IsExpired_Should_ReturnFalse_WhenExpirationInFuture()
    {
        // Arrange
        var notification = CreateNotification(expiresAt: DateTime.UtcNow.AddDays(7));

        // Assert
        notification.IsExpired.Should().BeFalse();
    }

    [TestMethod]
    public void IsExpired_Should_ReturnTrue_WhenExpirationInPast()
    {
        // Arrange
        var notification = CreateNotification(expiresAt: DateTime.UtcNow.AddDays(-1));

        // Assert
        notification.IsExpired.Should().BeTrue();
    }

    [TestMethod]
    public void IsActive_Should_ReturnTrue_WhenNotExpired()
    {
        // Arrange
        var notification = CreateNotification(expiresAt: DateTime.UtcNow.AddDays(7));

        // Assert
        notification.IsActive.Should().BeTrue();
    }

    [TestMethod]
    public void IsActive_Should_ReturnFalse_WhenExpired()
    {
        // Arrange
        var notification = CreateNotification(expiresAt: DateTime.UtcNow.AddDays(-1));

        // Assert
        notification.IsActive.Should().BeFalse();
    }

    [TestMethod]
    public void TimeToExpire_Should_ReturnNull_WhenNoExpirationSet()
    {
        // Arrange
        var notification = CreateNotification(expiresAt: null);

        // Assert
        notification.TimeToExpire.Should().BeNull();
    }

    [TestMethod]
    public void TimeToExpire_Should_ReturnPositiveTimeSpan_WhenExpirationInFuture()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddHours(5);
        var notification = CreateNotification(expiresAt: futureDate);

        // Assert
        notification.TimeToExpire.Should().NotBeNull();
        notification.TimeToExpire!.Value.TotalHours.Should().BeApproximately(5, 0.1);
    }

    [TestMethod]
    public void TimeToExpire_Should_ReturnNegativeTimeSpan_WhenExpired()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddHours(-2);
        var notification = CreateNotification(expiresAt: pastDate);

        // Assert
        notification.TimeToExpire.Should().NotBeNull();
        notification.TimeToExpire!.Value.TotalHours.Should().BeApproximately(-2, 0.1);
    }

    #endregion

    #region Business Methods Tests

    [TestMethod]
    public void MarkAsRead_Should_SetIsReadToTrue()
    {
        // Arrange
        var notification = CreateNotification(isRead: false);

        // Act
        notification.MarkAsRead();

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().NotBeNull();
        notification.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void MarkAsRead_Should_NotUpdateReadAt_WhenAlreadyRead()
    {
        // Arrange
        var originalReadAt = DateTime.UtcNow.AddHours(-1);
        var notification = CreateNotification(isRead: true);
        notification.ReadAt = originalReadAt;

        // Act
        notification.MarkAsRead();

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().Be(originalReadAt); // Should not change
    }

    [TestMethod]
    public void MarkAsUnread_Should_SetIsReadToFalse()
    {
        // Arrange
        var notification = CreateNotification(isRead: true);

        // Act
        notification.MarkAsUnread();

        // Assert
        notification.IsRead.Should().BeFalse();
        notification.ReadAt.Should().BeNull();
    }

    [TestMethod]
    public void MarkAsUnread_Should_ClearReadAt()
    {
        // Arrange
        var notification = CreateNotification(isRead: true);
        notification.ReadAt = DateTime.UtcNow;

        // Act
        notification.MarkAsUnread();

        // Assert
        notification.ReadAt.Should().BeNull();
    }

    [TestMethod]
    public void Expire_Should_SetExpiresAtToNow()
    {
        // Arrange
        var notification = CreateNotification(expiresAt: DateTime.UtcNow.AddDays(7));

        // Act
        notification.Expire();

        // Assert
        notification.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        notification.IsExpired.Should().BeTrue();
    }

    [TestMethod]
    public void Expire_Should_MakeNotificationInactive()
    {
        // Arrange
        var notification = CreateNotification(expiresAt: null);
        notification.IsActive.Should().BeTrue(); // Verify it's active before

        // Act
        notification.Expire();

        // Assert
        notification.IsActive.Should().BeFalse();
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void MarkAsRead_ThenUnread_Should_ResetState()
    {
        // Arrange
        var notification = CreateNotification(isRead: false);

        // Act
        notification.MarkAsRead();
        notification.MarkAsUnread();

        // Assert
        notification.IsRead.Should().BeFalse();
        notification.ReadAt.Should().BeNull();
    }

    [TestMethod]
    public void MultipleMarkAsRead_Should_BeIdempotent()
    {
        // Arrange
        var notification = CreateNotification(isRead: false);

        // Act
        notification.MarkAsRead();
        var firstReadAt = notification.ReadAt;
        
        Thread.Sleep(10); // Small delay
        notification.MarkAsRead();

        // Assert
        notification.ReadAt.Should().Be(firstReadAt); // Should not change
    }

    [TestMethod]
    public void Notification_Should_HandleAllTypes()
    {
        // Test all notification types
        var types = Enum.GetValues<NotificationType>();
        
        foreach (var type in types)
        {
            var notification = new Domain.Notification
            {
                NotificationId = 1,
                UserId = 1,
                Type = type,
                Title = $"Test {type}",
                Message = "Test message",
                Data = "{}",
                Priority = NotificationPriority.Normal,
                CreatedAt = DateTime.UtcNow
            };

            notification.Type.Should().Be(type);
        }
    }

    [TestMethod]
    public void Notification_Should_HandleAllPriorities()
    {
        // Test all priority levels
        var priorities = Enum.GetValues<NotificationPriority>();
        
        foreach (var priority in priorities)
        {
            var notification = new Domain.Notification
            {
                NotificationId = 1,
                UserId = 1,
                Type = NotificationType.System,
                Title = $"Test {priority}",
                Message = "Test message",
                Data = "{}",
                Priority = priority,
                CreatedAt = DateTime.UtcNow
            };

            notification.Priority.Should().Be(priority);
        }
    }

    #endregion
}
