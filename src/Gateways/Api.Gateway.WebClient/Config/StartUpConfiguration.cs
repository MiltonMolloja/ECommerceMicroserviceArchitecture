using Api.Gateway.Proxies;
using Api.Gateway.Proxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Gateway.WebClient.Config
{
    public static class StartUpConfiguration
    {
        public static IServiceCollection AddAppsettingBinding(this IServiceCollection service, IConfiguration configuration)
        {
            service.Configure<ApiUrls>(opts => configuration.GetSection("ApiUrls").Bind(opts));
            return service;
        }

        public static IServiceCollection AddProxiesRegistration(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddHttpContextAccessor();

            service.AddHttpClient<IOrderProxy, OrderProxy>();
            service.AddHttpClient<ICustomerProxy, CustomerProxy>();
            service.AddHttpClient<ICatalogProxy, CatalogProxy>();
            service.AddHttpClient<ICategoryProxy, CategoryProxy>();
            service.AddHttpClient<ICartProxy, CartProxy>();
            service.AddHttpClient<IPaymentProxy, PaymentProxy>();
            service.AddHttpClient<IHomeProxy, HomeProxy>();

            return service;
        }
    }
}
