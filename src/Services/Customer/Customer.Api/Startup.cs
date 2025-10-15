using Common.Caching;
using Common.Logging;
using Common.RateLimiting;
using Customer.Common;
using Customer.Persistence.Database;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Order.Service.Queries;
using System.Reflection;
using System.Text;

namespace Customer.Api
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
            // HttpContextAccessor
            services.AddHttpContextAccessor();

            // Redis Cache
            services.AddRedisCache(Configuration);

            // Rate Limiting
            services.AddCustomRateLimiting(Configuration);

            // DbContext
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory", "Customer")
                )
            );

            // Health check
            services.AddHealthChecks()
                        .AddCheck("self", () => HealthCheckResult.Healthy())
                        .AddDbContextCheck<ApplicationDbContext>(typeof(ApplicationDbContext).Name);

            // Health Checks UI
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(10); // Eval�a cada 10 segundos
                setup.MaximumHistoryEntriesPerEndpoint(50); // Mantiene historial de 50 entradas
            })
            .AddInMemoryStorage(); // Usa almacenamiento en memoria (puede cambiarse a SQL Server si se desea)

            // Event handlers
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("Customer.Service.EventHandlers")));

            // Cache Settings
            services.Configure<CacheSettings>(opts => Configuration.GetSection("CacheSettings").Bind(opts));

            // Query services
            services.AddTransient<IClientQueryService, ClientQueryService>();

            // API Controllers
            services.AddControllers();

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Customer API",
                    Version = "v1",
                    Description = "Microservicio de Customer - ECommerce Architecture"
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
                    }
                });
            });

            // Add Authentication
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
            // Database Logging - Enabled in all environments
            loggerFactory.AddDatabase(
                Configuration.GetConnectionString("DefaultConnection"),
                "Customer.Api");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Swagger UI
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer API v1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseRouting();

            // Rate Limiting
            app.UseCustomRateLimiting();

            app.UseAuthorization();
            app.UseAuthentication();

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
