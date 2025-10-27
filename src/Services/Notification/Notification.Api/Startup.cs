using Common.ApiKey;
using Common.Caching;
using Common.CorrelationId;
using Common.Logging;
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
using Microsoft.IdentityModel.Tokens;
using Notification.Persistence.Database;
using Notification.Service.Queries;
using System.Reflection;
using System.Text;

namespace Notification.Api
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

            // DbContext
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory", "Notification")
                )
            );

            // Health check
            services.AddHealthChecks()
                        .AddCheck("self", () => HealthCheckResult.Healthy())
                        .AddDbContextCheck<ApplicationDbContext>(typeof(ApplicationDbContext).Name);

            // Health Checks UI
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(10);
                setup.MaximumHistoryEntriesPerEndpoint(50);
            })
            .AddInMemoryStorage();

            // Event handlers
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("Notification.Service.EventHandlers")));

            // FluentValidation
            services.AddValidatorsFromAssembly(Assembly.Load("Notification.Service.EventHandlers"));
            services.AddValidationBehavior();

            // Query services
            services.AddTransient<INotificationQueryService, NotificationQueryService>();

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
                    Title = "Notification API",
                    Version = "v1",
                    Description = "Microservicio de Notification - ECommerce Architecture"
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
            // Database Logging
            var httpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
            loggerFactory.AddDatabase(
                Configuration.GetConnectionString("DefaultConnection"),
                "Notification.Api",
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification API v1");
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
