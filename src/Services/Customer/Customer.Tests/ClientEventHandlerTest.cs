using Customer.Domain;
using Customer.Service.EventHandlers;
using Customer.Service.EventHandlers.Commands;
using Customer.Tests.Config;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Customer.Tests;

[TestClass]
public class ClientEventHandlerTest
{
    #region ClientCreateCommand Tests

    [TestMethod]
    public async Task Should_CreateClient_When_ValidData()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new ClientEventHandler(context);
        var command = new ClientCreateCommand
        {
            UserId = "user-123",
            Phone = "+1234567890",
            PreferredLanguage = "en"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var client = await context.Clients.FirstOrDefaultAsync();
        client.Should().NotBeNull();
        client!.UserId.Should().Be("user-123");
        client.Phone.Should().Be("+1234567890");
        client.PreferredLanguage.Should().Be("en");
    }

    [TestMethod]
    public async Task Should_SetDefaultValues_When_CreatingClient()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new ClientEventHandler(context);
        var command = new ClientCreateCommand
        {
            UserId = "user-456",
            Phone = "+9876543210"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var client = await context.Clients.FirstOrDefaultAsync();
        client.Should().NotBeNull();
        client!.PreferredLanguage.Should().Be("es"); // Default language
        client.PreferredCurrency.Should().Be("USD");
        client.IsActive.Should().BeTrue();
        client.IsEmailVerified.Should().BeFalse();
        client.NewsletterSubscribed.Should().BeFalse();
    }

    [TestMethod]
    public async Task Should_SetTimestamps_When_CreatingClient()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new ClientEventHandler(context);
        var beforeCreate = DateTime.UtcNow.AddSeconds(-1);
        var command = new ClientCreateCommand
        {
            UserId = "user-789",
            Phone = "+1111111111"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var client = await context.Clients.FirstOrDefaultAsync();
        client.Should().NotBeNull();
        client!.CreatedAt.Should().BeAfter(beforeCreate);
        client.UpdatedAt.Should().BeAfter(beforeCreate);
    }

    #endregion

    #region ClientUpdateProfileCommand Tests

    [TestMethod]
    public async Task Should_UpdateProfile_When_ClientExists()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var existingClient = new Client
        {
            UserId = "user-update-1",
            Phone = "old-phone",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await context.Clients.AddAsync(existingClient);
        await context.SaveChangesAsync();

        var handler = new ClientEventHandler(context);
        var command = new ClientUpdateProfileCommand
        {
            ClientId = existingClient.ClientId,
            Phone = "new-phone",
            MobilePhone = "mobile-123",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M",
            ProfileImageUrl = "https://example.com/image.jpg"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedClient = await context.Clients.FindAsync(existingClient.ClientId);
        updatedClient.Should().NotBeNull();
        updatedClient!.Phone.Should().Be("new-phone");
        updatedClient.MobilePhone.Should().Be("mobile-123");
        updatedClient.DateOfBirth.Should().Be(new DateTime(1990, 5, 15));
        updatedClient.Gender.Should().Be("M");
        updatedClient.ProfileImageUrl.Should().Be("https://example.com/image.jpg");
    }

    [TestMethod]
    public async Task Should_NotThrow_When_UpdatingNonExistentClient()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new ClientEventHandler(context);
        var command = new ClientUpdateProfileCommand
        {
            ClientId = 999,
            Phone = "new-phone"
        };

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ClientUpdatePreferencesCommand Tests

    [TestMethod]
    public async Task Should_UpdatePreferences_When_ClientExists()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var existingClient = new Client
        {
            UserId = "user-pref-1",
            PreferredLanguage = "es",
            PreferredCurrency = "USD",
            NewsletterSubscribed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await context.Clients.AddAsync(existingClient);
        await context.SaveChangesAsync();

        var handler = new ClientEventHandler(context);
        var command = new ClientUpdatePreferencesCommand
        {
            ClientId = existingClient.ClientId,
            PreferredLanguage = "en",
            PreferredCurrency = "EUR",
            NewsletterSubscribed = true,
            SmsNotificationsEnabled = true,
            EmailNotificationsEnabled = true
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedClient = await context.Clients.FindAsync(existingClient.ClientId);
        updatedClient.Should().NotBeNull();
        updatedClient!.PreferredLanguage.Should().Be("en");
        updatedClient.PreferredCurrency.Should().Be("EUR");
        updatedClient.NewsletterSubscribed.Should().BeTrue();
        updatedClient.SmsNotificationsEnabled.Should().BeTrue();
        updatedClient.EmailNotificationsEnabled.Should().BeTrue();
    }

    #endregion

    #region ClientAddressCreateCommand Tests

    [TestMethod]
    public async Task Should_CreateAddress_When_ValidData()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var existingClient = new Client
        {
            UserId = "user-addr-1",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await context.Clients.AddAsync(existingClient);
        await context.SaveChangesAsync();

        var handler = new ClientEventHandler(context);
        var command = new ClientAddressCreateCommand
        {
            ClientId = existingClient.ClientId,
            AddressType = "Shipping",
            AddressName = "Home",
            RecipientName = "John Doe",
            RecipientPhone = "+1234567890",
            AddressLine1 = "123 Main St",
            AddressLine2 = "Apt 4B",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            IsDefaultShipping = true,
            IsDefaultBilling = false
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var address = await context.ClientAddresses.FirstOrDefaultAsync();
        address.Should().NotBeNull();
        address!.ClientId.Should().Be(existingClient.ClientId);
        address.AddressName.Should().Be("Home");
        address.City.Should().Be("New York");
        address.IsDefaultShipping.Should().BeTrue();
        address.IsActive.Should().BeTrue();
    }

    [TestMethod]
    public async Task Should_UnsetPreviousDefaultShipping_When_NewDefaultIsSet()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var existingClient = new Client
        {
            UserId = "user-addr-2",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await context.Clients.AddAsync(existingClient);
        await context.SaveChangesAsync();

        var existingAddress = new ClientAddress
        {
            ClientId = existingClient.ClientId,
            AddressType = "Shipping",
            AddressName = "Old Home",
            RecipientName = "John Doe",
            AddressLine1 = "Old Street",
            City = "Old City",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            IsDefaultShipping = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await context.ClientAddresses.AddAsync(existingAddress);
        await context.SaveChangesAsync();

        var handler = new ClientEventHandler(context);
        var command = new ClientAddressCreateCommand
        {
            ClientId = existingClient.ClientId,
            AddressType = "Shipping",
            AddressName = "New Home",
            RecipientName = "Jane Doe",
            AddressLine1 = "New Street",
            City = "New City",
            State = "CA",
            PostalCode = "90001",
            Country = "USA",
            IsDefaultShipping = true
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var oldAddress = await context.ClientAddresses.FindAsync(existingAddress.AddressId);
        oldAddress!.IsDefaultShipping.Should().BeFalse();

        var newAddress = await context.ClientAddresses
            .Where(a => a.AddressName == "New Home")
            .FirstOrDefaultAsync();
        newAddress!.IsDefaultShipping.Should().BeTrue();
    }

    #endregion

    #region ClientAddressUpdateCommand Tests

    [TestMethod]
    public async Task Should_UpdateAddress_When_AddressExists()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var existingAddress = new ClientAddress
        {
            ClientId = 1,
            AddressType = "Shipping",
            AddressName = "Old Name",
            RecipientName = "Old Recipient",
            AddressLine1 = "Old Street",
            City = "Old City",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await context.ClientAddresses.AddAsync(existingAddress);
        await context.SaveChangesAsync();

        var handler = new ClientEventHandler(context);
        var command = new ClientAddressUpdateCommand
        {
            AddressId = existingAddress.AddressId,
            AddressName = "New Name",
            RecipientName = "Jane Doe",
            AddressLine1 = "456 New St",
            City = "Los Angeles",
            State = "CA",
            PostalCode = "90001",
            Country = "USA"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedAddress = await context.ClientAddresses.FindAsync(existingAddress.AddressId);
        updatedAddress.Should().NotBeNull();
        updatedAddress!.AddressName.Should().Be("New Name");
        updatedAddress.RecipientName.Should().Be("Jane Doe");
        updatedAddress.City.Should().Be("Los Angeles");
    }

    #endregion

    #region ClientAddressSetDefaultCommand Tests

    [TestMethod]
    public async Task Should_SetDefaultShipping_When_AddressTypeIsShipping()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var clientId = 1;
        var address1 = new ClientAddress
        {
            ClientId = clientId,
            AddressType = "Shipping",
            AddressName = "Address 1",
            RecipientName = "Recipient 1",
            AddressLine1 = "Street 1",
            City = "City 1",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            IsDefaultShipping = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var address2 = new ClientAddress
        {
            ClientId = clientId,
            AddressType = "Shipping",
            AddressName = "Address 2",
            RecipientName = "Recipient 2",
            AddressLine1 = "Street 2",
            City = "City 2",
            State = "CA",
            PostalCode = "90001",
            Country = "USA",
            IsDefaultShipping = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await context.ClientAddresses.AddRangeAsync(address1, address2);
        await context.SaveChangesAsync();

        var handler = new ClientEventHandler(context);
        var command = new ClientAddressSetDefaultCommand
        {
            AddressId = address2.AddressId,
            AddressType = "Shipping"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedAddress1 = await context.ClientAddresses.FindAsync(address1.AddressId);
        var updatedAddress2 = await context.ClientAddresses.FindAsync(address2.AddressId);
        
        updatedAddress1!.IsDefaultShipping.Should().BeFalse();
        updatedAddress2!.IsDefaultShipping.Should().BeTrue();
    }

    [TestMethod]
    public async Task Should_SetDefaultBilling_When_AddressTypeIsBilling()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var clientId = 1;
        var address1 = new ClientAddress
        {
            ClientId = clientId,
            AddressType = "Billing",
            AddressName = "Address 1",
            RecipientName = "Recipient 1",
            AddressLine1 = "Street 1",
            City = "City 1",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            IsDefaultBilling = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var address2 = new ClientAddress
        {
            ClientId = clientId,
            AddressType = "Billing",
            AddressName = "Address 2",
            RecipientName = "Recipient 2",
            AddressLine1 = "Street 2",
            City = "City 2",
            State = "CA",
            PostalCode = "90001",
            Country = "USA",
            IsDefaultBilling = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await context.ClientAddresses.AddRangeAsync(address1, address2);
        await context.SaveChangesAsync();

        var handler = new ClientEventHandler(context);
        var command = new ClientAddressSetDefaultCommand
        {
            AddressId = address2.AddressId,
            AddressType = "Billing"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedAddress1 = await context.ClientAddresses.FindAsync(address1.AddressId);
        var updatedAddress2 = await context.ClientAddresses.FindAsync(address2.AddressId);
        
        updatedAddress1!.IsDefaultBilling.Should().BeFalse();
        updatedAddress2!.IsDefaultBilling.Should().BeTrue();
    }

    #endregion

    #region ClientAddressDeleteCommand Tests

    [TestMethod]
    public async Task Should_SoftDeleteAddress_When_AddressExists()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var existingAddress = new ClientAddress
        {
            ClientId = 1,
            AddressType = "Shipping",
            AddressName = "To Delete",
            RecipientName = "Delete Recipient",
            AddressLine1 = "Delete Street",
            City = "Delete City",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await context.ClientAddresses.AddAsync(existingAddress);
        await context.SaveChangesAsync();

        var handler = new ClientEventHandler(context);
        var command = new ClientAddressDeleteCommand
        {
            AddressId = existingAddress.AddressId
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedAddress = await context.ClientAddresses.FindAsync(existingAddress.AddressId);
        deletedAddress.Should().NotBeNull();
        deletedAddress!.IsActive.Should().BeFalse(); // Soft delete
    }

    #endregion
}
