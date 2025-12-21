using Customer.Domain;
using FluentAssertions;

namespace Customer.Tests;

[TestClass]
public class ClientDomainTest
{
    #region Age Calculation Tests

    [TestMethod]
    public void Age_Should_ReturnNull_When_DateOfBirthIsNull()
    {
        // Arrange
        var client = new Client
        {
            UserId = "user-1",
            DateOfBirth = null
        };

        // Act & Assert
        client.Age.Should().BeNull();
    }

    [TestMethod]
    public void Age_Should_CalculateCorrectly_When_DateOfBirthIsSet()
    {
        // Arrange
        var today = DateTime.Today;
        var birthDate = today.AddYears(-30);
        var client = new Client
        {
            UserId = "user-2",
            DateOfBirth = birthDate
        };

        // Act & Assert
        client.Age.Should().Be(30);
    }

    [TestMethod]
    public void Age_Should_NotCountBirthday_When_BirthdayHasNotOccurredThisYear()
    {
        // Arrange
        var today = DateTime.Today;
        var birthDate = today.AddYears(-30).AddDays(1); // Birthday is tomorrow
        var client = new Client
        {
            UserId = "user-3",
            DateOfBirth = birthDate
        };

        // Act & Assert
        client.Age.Should().Be(29);
    }

    #endregion

    #region UpdateProfile Tests

    [TestMethod]
    public void UpdateProfile_Should_UpdateAllFields()
    {
        // Arrange
        var client = new Client
        {
            UserId = "user-4",
            Phone = "old-phone",
            MobilePhone = "old-mobile",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M",
            ProfileImageUrl = "old-url"
        };
        var beforeUpdate = DateTime.UtcNow.AddSeconds(-1);

        // Act
        client.UpdateProfile(
            phone: "new-phone",
            mobilePhone: "new-mobile",
            dateOfBirth: new DateTime(1995, 5, 15),
            gender: "F",
            profileImageUrl: "new-url"
        );

        // Assert
        client.Phone.Should().Be("new-phone");
        client.MobilePhone.Should().Be("new-mobile");
        client.DateOfBirth.Should().Be(new DateTime(1995, 5, 15));
        client.Gender.Should().Be("F");
        client.ProfileImageUrl.Should().Be("new-url");
        client.UpdatedAt.Should().BeAfter(beforeUpdate);
    }

    #endregion

    #region UpdatePreferences Tests

    [TestMethod]
    public void UpdatePreferences_Should_UpdateAllFields()
    {
        // Arrange
        var client = new Client
        {
            UserId = "user-5",
            PreferredLanguage = "es",
            PreferredCurrency = "USD",
            NewsletterSubscribed = false
        };
        var beforeUpdate = DateTime.UtcNow.AddSeconds(-1);

        // Act
        client.UpdatePreferences(
            language: "en",
            currency: "EUR",
            newsletter: true
        );

        // Assert
        client.PreferredLanguage.Should().Be("en");
        client.PreferredCurrency.Should().Be("EUR");
        client.NewsletterSubscribed.Should().BeTrue();
        client.UpdatedAt.Should().BeAfter(beforeUpdate);
    }

    #endregion

    #region Verification Tests

    [TestMethod]
    public void VerifyEmail_Should_SetIsEmailVerifiedToTrue()
    {
        // Arrange
        var client = new Client
        {
            UserId = "user-6",
            IsEmailVerified = false
        };
        var beforeVerify = DateTime.UtcNow.AddSeconds(-1);

        // Act
        client.VerifyEmail();

        // Assert
        client.IsEmailVerified.Should().BeTrue();
        client.UpdatedAt.Should().BeAfter(beforeVerify);
    }

    [TestMethod]
    public void VerifyPhone_Should_SetIsPhoneVerifiedToTrue()
    {
        // Arrange
        var client = new Client
        {
            UserId = "user-7",
            IsPhoneVerified = false
        };
        var beforeVerify = DateTime.UtcNow.AddSeconds(-1);

        // Act
        client.VerifyPhone();

        // Assert
        client.IsPhoneVerified.Should().BeTrue();
        client.UpdatedAt.Should().BeAfter(beforeVerify);
    }

    #endregion

    #region Activation Tests

    [TestMethod]
    public void Activate_Should_SetIsActiveToTrue()
    {
        // Arrange
        var client = new Client
        {
            UserId = "user-8",
            IsActive = false
        };
        var beforeActivate = DateTime.UtcNow.AddSeconds(-1);

        // Act
        client.Activate();

        // Assert
        client.IsActive.Should().BeTrue();
        client.UpdatedAt.Should().BeAfter(beforeActivate);
    }

    [TestMethod]
    public void Deactivate_Should_SetIsActiveToFalse()
    {
        // Arrange
        var client = new Client
        {
            UserId = "user-9",
            IsActive = true
        };
        var beforeDeactivate = DateTime.UtcNow.AddSeconds(-1);

        // Act
        client.Deactivate();

        // Assert
        client.IsActive.Should().BeFalse();
        client.UpdatedAt.Should().BeAfter(beforeDeactivate);
    }

    #endregion

    #region RecordLogin Tests

    [TestMethod]
    public void RecordLogin_Should_UpdateLastLoginAt()
    {
        // Arrange
        var client = new Client
        {
            UserId = "user-10",
            LastLoginAt = null
        };
        var beforeLogin = DateTime.UtcNow.AddSeconds(-1);

        // Act
        client.RecordLogin();

        // Assert
        client.LastLoginAt.Should().NotBeNull();
        client.LastLoginAt.Should().BeAfter(beforeLogin);
    }

    #endregion
}
