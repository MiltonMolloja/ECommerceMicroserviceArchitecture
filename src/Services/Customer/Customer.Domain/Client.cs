using System;
using System.Collections.Generic;

namespace Customer.Domain
{
    /// <summary>
    /// Entidad principal de Customer que representa el perfil completo del usuario
    /// </summary>
    public class Client
    {
        #region Properties - Identificación

        public int ClientId { get; set; }

        /// <summary>
        /// ID del usuario en el sistema de identidad (AspNetUsers)
        /// </summary>
        public string UserId { get; set; }

        #endregion

        #region Properties - Información Personal

        // NOTA: FirstName, LastName y Email se obtienen de Identity.AspNetUsers via UserId
        // No se duplican aquí para evitar inconsistencias de datos

        public string Phone { get; set; }
        public string MobilePhone { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } // "M", "F", "Other", "PreferNotToSay"

        public string ProfileImageUrl { get; set; }

        #endregion

        #region Properties - Preferencias

        public string PreferredLanguage { get; set; } // "es", "en"
        public string PreferredCurrency { get; set; } // "USD", "EUR", etc.
        public bool NewsletterSubscribed { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
        public bool EmailNotificationsEnabled { get; set; }

        #endregion

        #region Properties - Estado

        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }

        #endregion

        #region Properties - Auditoría

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Direcciones del cliente (envío, facturación, etc.)
        /// </summary>
        public ICollection<ClientAddress> Addresses { get; set; } = new List<ClientAddress>();

        #endregion

        #region Computed Properties

        // NOTA: FullName, DisplayName se calculan en el DTO obteniendo datos de Identity
        // FirstName, LastName, Email no están en esta entidad

        /// <summary>
        /// Edad calculada desde la fecha de nacimiento
        /// </summary>
        public int? Age
        {
            get
            {
                if (!DateOfBirth.HasValue) return null;
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Value.Year;
                if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        #endregion

        #region Business Methods

        public void UpdateProfile(string phone, string mobilePhone, DateTime? dateOfBirth, string gender, string profileImageUrl)
        {
            Phone = phone;
            MobilePhone = mobilePhone;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            ProfileImageUrl = profileImageUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePreferences(string language, string currency, bool newsletter)
        {
            PreferredLanguage = language;
            PreferredCurrency = currency;
            NewsletterSubscribed = newsletter;
            UpdatedAt = DateTime.UtcNow;
        }

        public void VerifyEmail()
        {
            IsEmailVerified = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void VerifyPhone()
        {
            IsPhoneVerified = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }

        #endregion
    }
}
