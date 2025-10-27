using MediatR;
using System;

namespace Customer.Service.EventHandlers.Commands
{
    public class ClientCreateCommand : INotification
    {
        // NOTA: FirstName, LastName, Email se obtienen de Identity via UserId
        // No se duplican aquí
        public string UserId { get; set; }
        public string Phone { get; set; }
        public string PreferredLanguage { get; set; }
    }

    public class ClientUpdateProfileCommand : INotification
    {
        // NOTA: FirstName, LastName deben actualizarse en Identity.Api, no aquí
        public int ClientId { get; set; }
        public string Phone { get; set; }
        public string MobilePhone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ProfileImageUrl { get; set; }
    }

    public class ClientUpdatePreferencesCommand : INotification
    {
        public int ClientId { get; set; }
        public string PreferredLanguage { get; set; }
        public string PreferredCurrency { get; set; }
        public bool NewsletterSubscribed { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
        public bool EmailNotificationsEnabled { get; set; }
    }

    public class ClientAddressCreateCommand : INotification
    {
        public int ClientId { get; set; }
        public string AddressType { get; set; }
        public string AddressName { get; set; }
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsDefaultShipping { get; set; }
        public bool IsDefaultBilling { get; set; }
    }

    public class ClientAddressUpdateCommand : INotification
    {
        public int AddressId { get; set; }
        public string AddressName { get; set; }
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }

    public class ClientAddressSetDefaultCommand : INotification
    {
        public int AddressId { get; set; }
        public string AddressType { get; set; } // "Shipping" or "Billing"
    }

    public class ClientAddressDeleteCommand : INotification
    {
        public int AddressId { get; set; }
    }
}
