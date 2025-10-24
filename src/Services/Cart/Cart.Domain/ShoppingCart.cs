using System;
using System.Collections.Generic;
using System.Linq;

namespace Cart.Domain
{
    /// <summary>
    /// Estado del carrito de compras
    /// </summary>
    public enum CartStatus
    {
        Active = 1,      // Carrito activo
        Abandoned = 2,   // Carrito abandonado (más de X horas sin actividad)
        Converted = 3,   // Convertido a orden
        Expired = 4      // Expirado y eliminado
    }

    /// <summary>
    /// Carrito de compras de un cliente
    /// </summary>
    public class ShoppingCart
    {
        #region Properties - Persistidas

        public int CartId { get; set; }

        /// <summary>
        /// ID del cliente (null para carritos anónimos)
        /// </summary>
        public int? ClientId { get; set; }

        /// <summary>
        /// Session ID para carritos anónimos (GUID)
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Estado del carrito
        /// </summary>
        public CartStatus Status { get; set; }

        /// <summary>
        /// Código de cupón aplicado
        /// </summary>
        public string CouponCode { get; set; }

        /// <summary>
        /// Descuento del cupón (%)
        /// </summary>
        public decimal CouponDiscountPercentage { get; set; }

        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Última actualización
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Fecha de expiración (carritos anónimos expiran en 7 días)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Fecha de conversión a orden
        /// </summary>
        public DateTime? ConvertedAt { get; set; }

        /// <summary>
        /// ID de la orden generada (cuando se convierte)
        /// </summary>
        public int? OrderId { get; set; }

        // Navegación
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        #endregion

        #region Computed Properties - NO persistidas

        /// <summary>
        /// Subtotal sin descuentos ni impuestos
        /// </summary>
        public decimal Subtotal => Items?.Sum(i => i.LineTotal) ?? 0;

        /// <summary>
        /// Descuento total del cupón
        /// </summary>
        public decimal CouponDiscount => Subtotal * (CouponDiscountPercentage / 100);

        /// <summary>
        /// Subtotal después de aplicar cupón
        /// </summary>
        public decimal SubtotalAfterCoupon => Subtotal - CouponDiscount;

        /// <summary>
        /// Total de impuestos
        /// </summary>
        public decimal TaxTotal => Items?.Sum(i => i.TaxAmount) ?? 0;

        /// <summary>
        /// Total final del carrito
        /// </summary>
        public decimal Total => SubtotalAfterCoupon + TaxTotal;

        /// <summary>
        /// Cantidad total de items
        /// </summary>
        public int ItemCount => Items?.Sum(i => i.Quantity) ?? 0;

        /// <summary>
        /// Cantidad de productos únicos
        /// </summary>
        public int UniqueItemCount => Items?.Count ?? 0;

        /// <summary>
        /// Indica si el carrito está vacío
        /// </summary>
        public bool IsEmpty => Items == null || !Items.Any();

        /// <summary>
        /// Indica si es un carrito anónimo
        /// </summary>
        public bool IsAnonymous => !ClientId.HasValue;

        /// <summary>
        /// Indica si el carrito ha expirado
        /// </summary>
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

        /// <summary>
        /// Indica si tiene cupón aplicado
        /// </summary>
        public bool HasCoupon => !string.IsNullOrEmpty(CouponCode) && CouponDiscountPercentage > 0;

        #endregion

        #region Business Methods

        /// <summary>
        /// Agrega un item al carrito o incrementa la cantidad si ya existe
        /// </summary>
        public void AddItem(int productId, int quantity, decimal unitPrice, decimal taxRate, string productName)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0");

            if (unitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative");

            var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                // Incrementar cantidad del item existente
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            }
            else
            {
                // Agregar nuevo item
                var newItem = new CartItem
                {
                    CartId = CartId,
                    ProductId = productId,
                    ProductName = productName,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TaxRate = taxRate,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                Items.Add(newItem);
            }

            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Remueve un item del carrito
        /// </summary>
        public void RemoveItem(int productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                Items.Remove(item);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Limpia todos los items del carrito
        /// </summary>
        public void Clear()
        {
            Items.Clear();
            CouponCode = null;
            CouponDiscountPercentage = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Aplica un cupón de descuento
        /// </summary>
        public void ApplyCoupon(string couponCode, decimal discountPercentage)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
                throw new ArgumentException("Coupon code cannot be empty");

            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ArgumentException("Discount percentage must be between 0 and 100");

            CouponCode = couponCode;
            CouponDiscountPercentage = discountPercentage;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Remueve el cupón aplicado
        /// </summary>
        public void RemoveCoupon()
        {
            CouponCode = null;
            CouponDiscountPercentage = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marca el carrito como abandonado
        /// </summary>
        public void MarkAsAbandoned()
        {
            Status = CartStatus.Abandoned;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Convierte el carrito a orden
        /// </summary>
        public void ConvertToOrder(int orderId)
        {
            Status = CartStatus.Converted;
            OrderId = orderId;
            ConvertedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Asocia el carrito anónimo a un cliente (cuando hace login)
        /// </summary>
        public void AssignToClient(int clientId)
        {
            if (ClientId.HasValue)
                throw new InvalidOperationException("Cart is already assigned to a client");

            ClientId = clientId;
            SessionId = null; // Limpiar session ID
            ExpiresAt = null; // Los carritos de clientes no expiran
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
