using Payment.Domain;

namespace Payment.Service.Gateways
{
    public interface IPaymentGatewayFactory
    {
        IPaymentGateway GetGateway(PaymentMethod method);
    }
}
