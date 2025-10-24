using System;

namespace Cart.Domain
{
    /// <summary>
    /// Item individual dentro de un carrito de compras
    /// </summary>
    public class CartItem
    {
        #region Properties - Persistidas

        public int CartItemId { get; set; }
        public int CartId { get; set; }

        /// <summary>
        /// ID del producto (FK a Catalog.Product)
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Nombre del producto (snapshot para evitar joins)
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// SKU del producto (snapshot)
        /// </summary>
        public string ProductSKU { get; set; }

        /// <summary>
        /// URL de la imagen principal (snapshot)
        /// </summary>
        public string ProductImageUrl { get; set; }

        /// <summary>
        /// Cantidad de este producto
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Precio unitario (snapshot del precio al momento de agregar al carrito)
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Porcentaje de descuento del producto
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Tasa de impuesto (%)
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Fecha en que se agregó al carrito
        /// </summary>
        public DateTime AddedAt { get; set; }

        /// <summary>
        /// Última actualización (cambio de cantidad, precio, etc)
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        // Navegación
        public ShoppingCart Cart { get; set; }

        #endregion

        #region Computed Properties - NO persistidas

        /// <summary>
        /// Precio unitario después de aplicar descuento
        /// </summary>
        public decimal UnitPriceAfterDiscount => UnitPrice * (1 - DiscountPercentage / 100);

        /// <summary>
        /// Total de la línea sin impuestos (cantidad * precio con descuento)
        /// </summary>
        public decimal LineTotal => Quantity * UnitPriceAfterDiscount;

        /// <summary>
        /// Monto de impuesto por item
        /// </summary>
        public decimal TaxPerItem => UnitPriceAfterDiscount * (TaxRate / 100);

        /// <summary>
        /// Monto total de impuesto para esta línea
        /// </summary>
        public decimal TaxAmount => TaxPerItem * Quantity;

        /// <summary>
        /// Total de la línea incluyendo impuestos
        /// </summary>
        public decimal LineTotalWithTax => LineTotal + TaxAmount;

        /// <summary>
        /// Ahorro total por descuentos
        /// </summary>
        public decimal TotalSavings => (UnitPrice - UnitPriceAfterDiscount) * Quantity;

        /// <summary>
        /// Indica si este item tiene descuento
        /// </summary>
        public bool HasDiscount => DiscountPercentage > 0;

        #endregion

        #region Business Methods

        /// <summary>
        /// Actualiza la cantidad del item
        /// </summary>
        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0");

            Quantity = newQuantity;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Actualiza el precio (cuando cambia el precio del producto)
        /// </summary>
        public void UpdatePrice(decimal newPrice, decimal discountPercentage = 0)
        {
            if (newPrice < 0)
                throw new ArgumentException("Price cannot be negative");

            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ArgumentException("Discount must be between 0 and 100");

            UnitPrice = newPrice;
            DiscountPercentage = discountPercentage;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
