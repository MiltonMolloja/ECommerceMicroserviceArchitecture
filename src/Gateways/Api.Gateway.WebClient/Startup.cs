using Api.Gateway.Models;
using Api.Gateway.WebClient.Config;
using Api.Gateway.WebClient.Swagger;
using Common.ApiKey;
using Common.Caching;
using Common.CorrelationId;
using Common.Logging;
using Common.RateLimiting;
using Common.Validation;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace Api.Gateway.WebClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // HttpContextAccessor (needed for language detection)
            services.AddHttpContextAccessor();

            // Redis Cache
            services.AddRedisCache(Configuration);

            // Rate Limiting
            services.AddCustomRateLimiting(Configuration);

            // API Key Authentication
            services.AddApiKeyAuthentication(Configuration);

            // Correlation ID
            services.AddCorrelationId();

            // Cache Settings
            services.Configure<CacheSettings>(opts => Configuration.GetSection("CacheSettings").Bind(opts));

            // Language-Aware Cache Key Provider (Scoped for per-request)
            services.AddScoped<ILanguageAwareCacheKeyProvider, LanguageAwareCacheKeyProvider>();

            // Health check
            services.AddHealthChecks()
                        .AddCheck("self", () => HealthCheckResult.Healthy());

            // Health Checks UI
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(10); // Evalúa cada 10 segundos
                setup.MaximumHistoryEntriesPerEndpoint(50); // Mantiene historial de 50 entradas
            })
            .AddInMemoryStorage(); // Usa almacenamiento en memoria (puede cambiarse a SQL Server si se desea)

            services.AddAppsettingBinding(Configuration)
                    .AddProxiesRegistration(Configuration);

            // FluentValidation (NO usar AddValidationBehavior porque no hay MediatR en el Gateway)
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

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
                    // Fix circular reference issue
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "API Gateway WebClient",
                    Version = "v1",
                    Description = "Gateway de ECommerce Architecture - Soporte multiidioma con Accept-Language header (es, en)"
                });

                // Configuración de seguridad JWT
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Ingrese el token JWT en el formato: Bearer {token}"
                });

                // Configuración de seguridad API Key
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

                // Agregar Accept-Language como parámetro global
                c.OperationFilter<AcceptLanguageHeaderOperationFilter>();
            });

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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                loggerFactory.AddSyslog(
                    Configuration.GetValue<string>("Papertrail:host"),
                    Configuration.GetValue<int>("Papertrail:port"));
            }

            // Validation exception handler
            app.UseValidationExceptionHandler();

            // CORS debe ir antes de UseRouting
            app.UseCors("AllowAll");

            app.UseRouting();

            // Swagger UI - Disponible en todos los ambientes sin autenticación
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway WebClient v1");
                c.RoutePrefix = "swagger";
            });

            // Correlation ID debe estar al principio para rastrear toda la petición
            app.UseCorrelationId();

            // API Key Authentication (debe ir antes de Rate Limiting)
            app.UseApiKeyAuthentication();

            // Rate Limiting debe estar antes de Authorization
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
