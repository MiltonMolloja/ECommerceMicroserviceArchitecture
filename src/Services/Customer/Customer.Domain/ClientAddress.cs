using System;

namespace Customer.Domain
{
    /// <summary>
    /// Entidad que representa una dirección del cliente (envío, facturación, etc.)
    /// </summary>
    public class ClientAddress
    {
        #region Properties - Identificación

        public int AddressId { get; set; }
        public int ClientId { get; set; }

        #endregion

        #region Properties - Tipo y Nombre

        /// <summary>
        /// Tipo de dirección: "Shipping", "Billing", "Both"
        /// </summary>
        public string AddressType { get; set; }

        /// <summary>
        /// Nombre descriptivo de la dirección (ej: "Casa", "Oficina", "Casa de verano")
        /// </summary>
        public string AddressName { get; set; }

        #endregion

        #region Properties - Información de Contacto

        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }

        #endregion

        #region Properties - Dirección

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        #endregion

        #region Properties - Configuración

        /// <summary>
        /// Indica si esta es la dirección predeterminada para envíos
        /// </summary>
        public bool IsDefaultShipping { get; set; }

        /// <summary>
        /// Indica si esta es la dirección predeterminada para facturación
        /// </summary>
        public bool IsDefaultBilling { get; set; }

        public bool IsActive { get; set; }

        #endregion

        #region Properties - Auditoría

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        #endregion

        #region Navigation Properties

        public Client Client { get; set; }

        #endregion

        #region Computed Properties

        /// <summary>
        /// Dirección completa formateada
        /// </summary>
        public string FullAddress
        {
            get
            {
                var parts = new[]
                {
                    AddressLine1,
                    AddressLine2,
                    City,
                    State,
                    PostalCode,
                    Country
                };

                return string.Join(", ", Array.FindAll(parts, s => !string.IsNullOrWhiteSpace(s)));
            }
        }

        /// <summary>
        /// Dirección resumida (línea 1 + ciudad)
        /// </summary>
        public string ShortAddress => $"{AddressLine1}, {City}";

        #endregion

        #region Business Methods

        public void SetAsDefaultShipping()
        {
            IsDefaultShipping = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAsDefaultBilling()
        {
            IsDefaultBilling = true;
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

        #endregion
    }
}
