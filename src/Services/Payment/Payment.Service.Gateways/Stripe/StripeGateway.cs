using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Threading.Tasks;

namespace Payment.Service.Gateways.Stripe
{
    public class StripeGateway : IPaymentGateway
    {
        private readonly IConfiguration _configuration;

        public StripeGateway(IConfiguration configuration)
        {
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100), // Stripe usa centavos
                    Currency = request.Currency.ToLower(),
                    PaymentMethod = request.PaymentToken,
                    Confirm = true,
                    Description = request.Description,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                        AllowRedirects = "never"
                    },
                    Expand = new System.Collections.Generic.List<string> { "payment_method" }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                if (paymentIntent.Status == "succeeded")
                {
                    // PaymentMethod is now expanded and available directly
                    var paymentMethod = paymentIntent.PaymentMethod;

                    return new PaymentResult
                    {
                        Success = true,
                        TransactionId = paymentIntent.Id,
                        Gateway = "Stripe",
                        CardLast4 = paymentMethod?.Card?.Last4,
                        CardBrand = paymentMethod?.Card?.Brand
                    };
                }
                else
                {
                    return new PaymentResult
                    {
                        Success = false,
                        ErrorMessage = $"Payment failed with status: {paymentIntent.Status}"
                    };
                }
            }
            catch (StripeException ex)
            {
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefundResult> ProcessRefundAsync(RefundRequest request)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = request.TransactionId,
                    Amount = (long)(request.Amount * 100),
                    Reason = request.Reason
                };

                var service = new RefundService();
                var refund = await service.CreateAsync(options);

                return new RefundResult
                {
                    Success = refund.Status == "succeeded",
                    RefundId = refund.Id,
                    ErrorMessage = refund.Status != "succeeded" ? $"Refund status: {refund.Status}" : null
                };
            }
            catch (StripeException ex)
            {
                return new RefundResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
