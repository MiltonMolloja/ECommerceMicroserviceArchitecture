using System;
using System.Collections.Generic;

namespace Api.Gateway.Models.Customer.DTOs
{
    /// <summary>
    /// DTO completo del perfil de usuario/cliente
    /// </summary>
    public class ClientDto
    {
        // Identificación
        public int ClientId { get; set; }
        public string UserId { get; set; }

        // Información Personal
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string MobilePhone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string Gender { get; set; }
        public string ProfileImageUrl { get; set; }

        // Preferencias
        public string PreferredLanguage { get; set; }
        public string PreferredCurrency { get; set; }
        public bool NewsletterSubscribed { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
        public bool EmailNotificationsEnabled { get; set; }

        // Estado
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }

        // Auditoría
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Direcciones
        public List<ClientAddressDto> Addresses { get; set; }
        public ClientAddressDto DefaultShippingAddress { get; set; }
        public ClientAddressDto DefaultBillingAddress { get; set; }
    }

    /// <summary>
    /// DTO de dirección del cliente
    /// </summary>
    public class ClientAddressDto
    {
        public int AddressId { get; set; }
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

        public string FullAddress { get; set; }
        public string ShortAddress { get; set; }

        public bool IsDefaultShipping { get; set; }
        public bool IsDefaultBilling { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO simplificado para listados
    /// </summary>
    public class ClientSummaryDto
    {
        public int ClientId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
