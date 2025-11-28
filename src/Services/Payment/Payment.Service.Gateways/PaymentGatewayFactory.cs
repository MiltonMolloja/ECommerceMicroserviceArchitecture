using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Domain;
using Payment.Service.Gateways.Mock;
using Payment.Service.Gateways.Stripe;
using Payment.Service.Gateways.MercadoPago;
using System;

namespace Payment.Service.Gateways
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public PaymentGatewayFactory(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public IPaymentGateway GetGateway(PaymentMethod method)
        {
            // Leer el provider configurado (Mock o Stripe)
            var provider = _configuration.GetValue<string>("PaymentGateway:Provider", "Mock");

            // Si está configurado como Mock, siempre usar MockGateway
            if (provider.Equals("Mock", StringComparison.OrdinalIgnoreCase))
            {
                return _serviceProvider.GetRequiredService<MockPaymentGateway>();
            }

            // Si está configurado como Stripe, usar lógica basada en el método de pago
            if (provider.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
            {
                return method switch
                {
                    PaymentMethod.CreditCard => _serviceProvider.GetRequiredService<StripeGateway>(),
                    PaymentMethod.DebitCard => _serviceProvider.GetRequiredService<StripeGateway>(),
                    PaymentMethod.Stripe => _serviceProvider.GetRequiredService<StripeGateway>(),
                    _ => throw new NotSupportedException($"Payment method {method} is not supported with Stripe gateway")
                };
            }

            // Si está configurado como MercadoPago, usar lógica basada en el método de pago
            if (provider.Equals("MercadoPago", StringComparison.OrdinalIgnoreCase))
            {
                return method switch
                {
                    PaymentMethod.CreditCard => _serviceProvider.GetRequiredService<MercadoPagoGateway>(),
                    PaymentMethod.DebitCard => _serviceProvider.GetRequiredService<MercadoPagoGateway>(),
                    PaymentMethod.MercadoPago => _serviceProvider.GetRequiredService<MercadoPagoGateway>(),
                    _ => throw new NotSupportedException($"Payment method {method} is not supported with MercadoPago gateway")
                };
            }

            throw new NotSupportedException($"Payment gateway provider '{provider}' is not supported. Use 'Mock', 'Stripe', or 'MercadoPago'.");
        }
    }
}
