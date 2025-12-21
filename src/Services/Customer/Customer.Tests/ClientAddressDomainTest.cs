using Customer.Domain;
using FluentAssertions;

namespace Customer.Tests;

[TestClass]
public class ClientAddressDomainTest
{
    #region FullAddress Tests

    [TestMethod]
    public void FullAddress_Should_ReturnFormattedAddress_WithAllFields()
    {
        // Arrange
        var address = new ClientAddress
        {
            AddressLine1 = "123 Main St",
            AddressLine2 = "Apt 4B",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA"
        };

        // Act
        var fullAddress = address.FullAddress;

        // Assert
        fullAddress.Should().Be("123 Main St, Apt 4B, New York, NY, 10001, USA");
    }

    [TestMethod]
    public void FullAddress_Should_SkipEmptyFields()
    {
        // Arrange
        var address = new ClientAddress
        {
            AddressLine1 = "123 Main St",
            AddressLine2 = null,
            City = "New York",
            State = "",
            PostalCode = "10001",
            Country = "USA"
        };

        // Act
        var fullAddress = address.FullAddress;

        // Assert
        fullAddress.Should().Be("123 Main St, New York, 10001, USA");
    }

    #endregion

    #region ShortAddress Tests

    [TestMethod]
    public void ShortAddress_Should_ReturnAddressLine1AndCity()
    {
        // Arrange
        var address = new ClientAddress
        {
            AddressLine1 = "456 Oak Ave",
            City = "Los Angeles"
        };

        // Act
        var shortAddress = address.ShortAddress;

        // Assert
        shortAddress.Should().Be("456 Oak Ave, Los Angeles");
    }

    #endregion

    #region SetAsDefaultShipping Tests

    [TestMethod]
    public void SetAsDefaultShipping_Should_SetFlagAndUpdateTimestamp()
    {
        // Arrange
        var address = new ClientAddress
        {
            IsDefaultShipping = false,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var beforeSet = DateTime.UtcNow.AddSeconds(-1);

        // Act
        address.SetAsDefaultShipping();

        // Assert
        address.IsDefaultShipping.Should().BeTrue();
        address.UpdatedAt.Should().BeAfter(beforeSet);
    }

    #endregion

    #region SetAsDefaultBilling Tests

    [TestMethod]
    public void SetAsDefaultBilling_Should_SetFlagAndUpdateTimestamp()
    {
        // Arrange
        var address = new ClientAddress
        {
            IsDefaultBilling = false,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var beforeSet = DateTime.UtcNow.AddSeconds(-1);

        // Act
        address.SetAsDefaultBilling();

        // Assert
        address.IsDefaultBilling.Should().BeTrue();
        address.UpdatedAt.Should().BeAfter(beforeSet);
    }

    #endregion

    #region Activate/Deactivate Tests

    [TestMethod]
    public void Activate_Should_SetIsActiveToTrue()
    {
        // Arrange
        var address = new ClientAddress
        {
            IsActive = false,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var beforeActivate = DateTime.UtcNow.AddSeconds(-1);

        // Act
        address.Activate();

        // Assert
        address.IsActive.Should().BeTrue();
        address.UpdatedAt.Should().BeAfter(beforeActivate);
    }

    [TestMethod]
    public void Deactivate_Should_SetIsActiveToFalse()
    {
        // Arrange
        var address = new ClientAddress
        {
            IsActive = true,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var beforeDeactivate = DateTime.UtcNow.AddSeconds(-1);

        // Act
        address.Deactivate();

        // Assert
        address.IsActive.Should().BeFalse();
        address.UpdatedAt.Should().BeAfter(beforeDeactivate);
    }

    #endregion
}
