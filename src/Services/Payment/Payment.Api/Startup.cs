using Common.ApiKey;
using Common.Caching;
using Common.CorrelationId;
using Common.Database;
using Common.Logging;
using Common.Messaging.Extensions;
using Common.RateLimiting;
using Common.Validation;
using FluentValidation;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Payment.Persistence.Database;
using Payment.Service.EventHandlers.Handlers;
using Payment.Service.Gateways;
using Payment.Service.Gateways.Stripe;
using Payment.Service.Proxies.Notification;
using Payment.Service.Proxies.Order;
using Payment.Service.Queries;
using System.Reflection;
using System.Text;

namespace Payment.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // HttpContextAccessor
            services.AddHttpContextAccessor();

            // Redis Cache
            services.AddRedisCache(Configuration);

            // Rate Limiting
            services.AddCustomRateLimiting(Configuration);

            // API Key Authentication
            services.AddApiKeyAuthentication(Configuration);

            // Correlation ID
            services.AddCorrelationId();

            // DbContext - Supports both SQL Server and PostgreSQL based on configuration
            services.AddDatabaseContext<ApplicationDbContext>(Configuration, "Payment");

// Health check
            services.AddHealthChecks()
                        .AddCheck("self", () => HealthCheckResult.Healthy())
                        .AddDbContextCheck<ApplicationDbContext>(typeof(ApplicationDbContext).Name)
                        .AddRabbitMQHealthCheck(Configuration);

            // Health Checks UI
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(10);
                setup.MaximumHistoryEntriesPerEndpoint(50);
            })
            .AddInMemoryStorage();

            // RabbitMQ Messaging (Publisher only - no consumers in Payment.Api)
            services.AddRabbitMQPublisher(Configuration);

            // Event handlers
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("Payment.Service.EventHandlers")));

            // FluentValidation
            services.AddValidatorsFromAssembly(Assembly.Load("Payment.Service.EventHandlers"));
            services.AddValidationBehavior();

            // Query services
            services.AddTransient<IPaymentQueryService, PaymentQueryService>();

            // Payment Gateways
            services.AddScoped<StripeGateway>();
            services.AddScoped<Payment.Service.Gateways.Mock.MockPaymentGateway>();
            services.AddHttpClient<Payment.Service.Gateways.MercadoPago.MercadoPagoGateway>();
            services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

            // Proxies (HttpClient)
            services.AddHttpClient<IOrderProxy, OrderProxy>()
                .ConfigureHttpClient(client =>
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", "payment-api-key-2025-secure-ecommerce-service-communication");
                });
            services.AddHttpClient<INotificationProxy, NotificationProxy>()
                .ConfigureHttpClient(client =>
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", "payment-api-key-2025-secure-ecommerce-service-communication");
                });
            services.AddHttpClient<Service.Proxies.Customer.ICustomerProxy, Service.Proxies.Customer.CustomerProxy>()
                .ConfigureHttpClient(client =>
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", "payment-api-key-2025-secure-ecommerce-service-communication");
                });

            // CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // API Controllers
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Payment API",
                    Version = "v1",
                    Description = "Microservicio de Payment - ECommerce Architecture"
                });

                // JWT Security
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Ingrese el token JWT en el formato: Bearer {token}"
                });

                // API Key Security
                c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "X-Api-Key",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Ingrese el API Key para comunicación entre servicios"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    },
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // Authentication
            var secretKey = Encoding.ASCII.GetBytes(
                Configuration.GetValue<string>("SecretKey")
            );

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            // Auto-create database schema on startup (for PostgreSQL compatibility)
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
                try
                {
                    db.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS \"Payment\"");
                    var created = db.Database.EnsureCreated();
                    logger.LogInformation("Database EnsureCreated for Payment returned: {Created}", created);
                }
                catch (System.Exception ex)
                {
                    logger.LogError(ex, "Failed to create Payment database schema");
                }
            }

            // Database Logging
            var httpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
            loggerFactory.AddDatabase(
                Configuration.GetConnectionString("DefaultConnection"),
                "Payment.Api",
                httpContextAccessor);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Validation exception handler
            app.UseValidationExceptionHandler();

            // CORS debe ir antes de UseRouting
            app.UseCors("AllowAll");

            app.UseRouting();

            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment API v1");
                c.RoutePrefix = "swagger";
            });

            // Correlation ID
            app.UseCorrelationId();

            // API Key Authentication
            app.UseApiKeyAuthentication();

            // Rate Limiting
            app.UseCustomRateLimiting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI();
            });
        }
    }
}
