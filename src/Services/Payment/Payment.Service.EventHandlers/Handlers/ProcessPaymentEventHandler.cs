using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Payment.Persistence.Database;
using Payment.Service.EventHandlers.Commands;
using Payment.Service.Gateways;
using Payment.Service.Proxies.Order;
using Payment.Service.Proxies.Notification;
using Payment.Service.Proxies.Customer;
using Common.Messaging.Events.Payments;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Payment.Service.EventHandlers.Handlers
{
    public class ProcessPaymentEventHandler : IRequestHandler<ProcessPaymentCommand, PaymentProcessingResult>
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly IOrderProxy _orderProxy;
        private readonly INotificationProxy _notificationProxy;
        private readonly ICustomerProxy _customerProxy;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ProcessPaymentEventHandler> _logger;

        public ProcessPaymentEventHandler(
            ApplicationDbContext context,
            IPaymentGatewayFactory gatewayFactory,
            IOrderProxy orderProxy,
            INotificationProxy notificationProxy,
            ICustomerProxy customerProxy,
            IPublishEndpoint publishEndpoint,
            ILogger<ProcessPaymentEventHandler> logger)
        {
            _context = context;
            _gatewayFactory = gatewayFactory;
            _orderProxy = orderProxy;
            _notificationProxy = notificationProxy;
            _customerProxy = customerProxy;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<PaymentProcessingResult> Handle(ProcessPaymentCommand notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"--- Processing payment for order {notification.OrderId}");

            try
            {
                // 1. Validar que la orden existe y obtener el monto
                var order = await _orderProxy.GetOrderByIdAsync(notification.OrderId);
                if (order == null)
                {
                    _logger.LogWarning($"Order {notification.OrderId} not found");
                    return new PaymentProcessingResult
                    {
                        Success = false,
                        Message = "Order not found",
                        ErrorMessage = "Order not found",
                        ErrorCode = "ORDER_NOT_FOUND"
                    };
                }

                // Determinar PaymentMethod basado en PaymentMethodId de MercadoPago
                var paymentMethod = Domain.PaymentMethod.MercadoPago;  // Siempre usar MercadoPago

                // 2. Crear registro de pago
                var payment = new Domain.Payment
                {
                    OrderId = notification.OrderId,
                    UserId = notification.UserId,
                    Amount = order.Total,  // Obtener el monto de la orden
                    Currency = "ARS",      // Moneda de Argentina
                    Status = Domain.PaymentStatus.Processing,
                    PaymentMethod = paymentMethod,
                    TransactionId = string.Empty, // Se actualizará después de procesar
                    PaymentGateway = "MercadoPago",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Payments.AddAsync(payment, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // 3. Obtener email del usuario desde el command (viene del token JWT)
                var userEmail = notification.UserEmail;

                // Si no viene en el command, intentar obtener del Customer Service como fallback
                string customerName = "Cliente";
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning($"UserEmail not found in command for user {notification.UserId}, attempting to get from Customer Service");
                    var customer = await _customerProxy.GetCustomerByIdAsync(order.ClientId);
                    userEmail = customer?.Email ?? "mfmolloja@gmail.com";
                    customerName = customer?.FullName ?? "Cliente";
                }
                else
                {
                    // Obtener nombre del cliente desde Customer Service
                    var customer = await _customerProxy.GetCustomerByIdAsync(order.ClientId);
                    customerName = customer?.FullName ?? "Cliente";
                }

                // 4. Procesar pago con MercadoPago
                var gateway = _gatewayFactory.GetGateway(paymentMethod);
                var result = await gateway.ProcessPaymentAsync(new PaymentRequest
                {
                    Amount = order.Total,
                    Currency = "ARS",
                    PaymentToken = notification.Token,
                    PaymentMethodId = notification.PaymentMethodId,
                    Installments = notification.Installments,
                    PayerEmail = userEmail,
                    CardholderName = notification.CardholderName,
                    IdentificationType = notification.IdentificationType,
                    IdentificationNumber = notification.IdentificationNumber,
                    Description = $"Order {notification.OrderId}"
                });

                // 4. Actualizar estado del pago
                if (result.Success)
                {
                    payment.MarkAsCompleted(result.TransactionId);
                    payment.PaymentGateway = result.Gateway;

                    // Guardar detalles del pago
                    payment.PaymentDetail = new Domain.PaymentDetail
                    {
                        CardLast4Digits = result.CardLast4,
                        CardBrand = result.CardBrand,
                        BillingAddress = notification.BillingAddress,
                        BillingCity = notification.BillingCity,
                        BillingCountry = notification.BillingCountry,
                        BillingZipCode = notification.BillingZipCode
                    };

                    // ========================================
                    // PUBLICAR EVENTO PaymentCompleted a RabbitMQ
                    // Los consumidores (Order.Service, Notification.Service) procesarán el evento
                    // ========================================
                    var paymentCompletedEvent = new PaymentCompletedEvent
                    {
                        PaymentId = payment.PaymentId,
                        OrderId = notification.OrderId,
                        ClientId = order.ClientId,
                        ClientEmail = userEmail,
                        ClientName = customerName,
                        Amount = order.Total,
                        PaymentMethod = result.Gateway ?? "MercadoPago",
                        TransactionId = result.TransactionId,
                        PaidAt = DateTime.UtcNow
                    };

                    await _publishEndpoint.Publish(paymentCompletedEvent, cancellationToken);
                    _logger.LogInformation("Published PaymentCompletedEvent for OrderId: {OrderId}, PaymentId: {PaymentId}", 
                        notification.OrderId, payment.PaymentId);

                    // ========================================
                    // FALLBACK: Mantener llamadas HTTP síncronas por ahora
                    // TODO: Remover cuando los consumidores estén funcionando correctamente
                    // ========================================
                    try
                    {
                        // Notificar al OrderService con transaction ID y gateway
                        await _orderProxy.UpdateOrderPaymentStatusAsync(
                            notification.OrderId,
                            "Paid",
                            result.TransactionId,
                            result.Gateway);

                        // Enviar notificación de confirmación de compra con todos los detalles
                        await _notificationProxy.SendOrderPlacedNotificationAsync(new Proxies.Notification.OrderPlacedNotification
                        {
                            UserId = notification.UserId,
                            UserEmail = userEmail,
                            CustomerName = customerName,
                            OrderNumber = $"#ORD-{notification.OrderId:D8}",
                            Items = new List<Proxies.Notification.OrderItemNotification>
                            {
                                // TODO: Obtener items reales de la orden
                                new Proxies.Notification.OrderItemNotification
                                {
                                    ProductName = "Producto de la orden",
                                    Quantity = 1,
                                    UnitPrice = FormatCurrency(order.Total)
                                }
                            },
                            Subtotal = FormatCurrency(order.Total * 0.87m), // Aproximado
                            ShippingCost = "Gratis",
                            Tax = FormatCurrency(order.Total * 0.13m), // Aproximado
                            Total = FormatCurrency(order.Total),
                            EstimatedDelivery = "3-5 días hábiles"
                        });
                    }
                    catch (Exception httpEx)
                    {
                        // Si falla HTTP, el evento ya fue publicado en RabbitMQ
                        // Los consumidores procesarán el evento de forma asíncrona
                        _logger.LogWarning(httpEx, "HTTP fallback failed, but event was published to RabbitMQ");
                    }

                    _logger.LogInformation($"Payment {payment.PaymentId} completed successfully");

                    await _context.SaveChangesAsync(cancellationToken);

                    return new PaymentProcessingResult
                    {
                        Success = true,
                        Message = "Payment processed successfully",
                        PaymentId = payment.PaymentId,
                        TransactionId = result.TransactionId
                    };
                }
                else
                {
                    payment.MarkAsFailed(result.ErrorMessage);

                    // ========================================
                    // PUBLICAR EVENTO PaymentFailed a RabbitMQ
                    // ========================================
                    var paymentFailedEvent = new PaymentFailedEvent
                    {
                        PaymentId = payment.PaymentId,
                        OrderId = notification.OrderId,
                        ClientId = order.ClientId,
                        ClientEmail = userEmail,
                        Amount = order.Total,
                        ErrorCode = result.ErrorCode,
                        ErrorMessage = result.ErrorMessage ?? "Payment processing failed",
                        FailedAt = DateTime.UtcNow
                    };

                    await _publishEndpoint.Publish(paymentFailedEvent, cancellationToken);
                    _logger.LogInformation("Published PaymentFailedEvent for OrderId: {OrderId}", notification.OrderId);

                    // ========================================
                    // FALLBACK: Mantener llamadas HTTP síncronas
                    // ========================================
                    try
                    {
                        // Notificar al OrderService que el pago falló
                        await _orderProxy.UpdateOrderPaymentStatusAsync(notification.OrderId, "PaymentFailed");

                        // Formatear método de pago
                        var paymentMethodDisplay = result.CardBrand != null && result.CardLast4 != null
                            ? $"{result.CardBrand} terminada en {result.CardLast4}"
                            : "MercadoPago";

                        // Notificar al usuario del fallo con todos los detalles
                        await _notificationProxy.SendPaymentFailedAsync(new Proxies.Notification.PaymentFailedNotification
                        {
                            UserId = notification.UserId,
                            PaymentId = payment.PaymentId,
                            Reason = result.ErrorMessage,
                            Email = userEmail,
                            CustomerName = customerName,
                            OrderNumber = $"#ORD-{notification.OrderId:D8}",
                            AttemptDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            Amount = FormatCurrency(order.Total),
                            PaymentMethod = paymentMethodDisplay,
                            FailureReason = result.ErrorMessage ?? "No pudimos procesar el pago. Por favor intenta nuevamente o contacta con soporte."
                        });
                    }
                    catch (Exception httpEx)
                    {
                        _logger.LogWarning(httpEx, "HTTP fallback failed, but event was published to RabbitMQ");
                    }

                    _logger.LogWarning($"Payment failed: {result.ErrorMessage}");

                    await _context.SaveChangesAsync(cancellationToken);

                    return new PaymentProcessingResult
                    {
                        Success = false,
                        Message = "Payment failed",
                        PaymentId = payment.PaymentId,
                        ErrorMessage = result.ErrorMessage,
                        ErrorCode = result.ErrorCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing payment for order {notification.OrderId}");
                return new PaymentProcessingResult
                {
                    Success = false,
                    Message = "Payment processing error",
                    ErrorMessage = ex.Message,
                    ErrorCode = "PROCESSING_ERROR"
                };
            }
        }

        private string FormatCurrency(decimal amount)
        {
            return amount.ToString("C2", new System.Globalization.CultureInfo("es-AR"));
        }
    }
}
