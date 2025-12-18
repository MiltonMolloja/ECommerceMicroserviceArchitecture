using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cart.Persistence.Database;
using Common.Messaging.Events.Cart;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cart.Api.BackgroundServices;

/// <summary>
/// Background service que detecta carritos abandonados y publica eventos.
/// Un carrito se considera abandonado si no ha tenido actividad en las últimas 24 horas.
/// </summary>
public class CartAbandonmentService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CartAbandonmentService> _logger;
    
    // Configuración
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Verificar cada hora
    private readonly TimeSpan _abandonmentThreshold = TimeSpan.FromHours(24); // 24 horas sin actividad

    public CartAbandonmentService(
        IServiceProvider serviceProvider,
        ILogger<CartAbandonmentService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CartAbandonmentService started. Check interval: {Interval}, Abandonment threshold: {Threshold}",
            _checkInterval, _abandonmentThreshold);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckForAbandonedCartsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for abandoned carts");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckForAbandonedCartsAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking for abandoned carts...");

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var abandonmentCutoff = DateTime.UtcNow.Subtract(_abandonmentThreshold);

        // Buscar carritos que:
        // 1. Tienen items
        // 2. No han sido actualizados en las últimas 24 horas
        // 3. No han sido notificados previamente (AbandonmentNotifiedAt es null)
        var abandonedCarts = await context.ShoppingCarts
            .Include(c => c.Items)
            .Where(c => c.Items.Any() && 
                        c.UpdatedAt < abandonmentCutoff &&
                        c.AbandonmentNotifiedAt == null &&
                        c.ClientId.HasValue) // Solo carritos de clientes registrados
            .ToListAsync(cancellationToken);

        if (!abandonedCarts.Any())
        {
            _logger.LogDebug("No abandoned carts found");
            return;
        }

        _logger.LogInformation("Found {Count} abandoned carts", abandonedCarts.Count);

        foreach (var cart in abandonedCarts)
        {
            try
            {
                var cartAbandonedEvent = new CartAbandonedEvent
                {
                    CartId = cart.CartId,
                    ClientId = cart.ClientId ?? 0,
                    ClientEmail = string.Empty, // TODO: Obtener del servicio de Customer/Identity
                    ClientName = string.Empty,
                    Total = cart.Items.Sum(i => i.Quantity * i.UnitPrice),
                    ItemCount = cart.Items.Count,
                    Items = cart.Items.Select(i => new CartItemInfo
                    {
                        ProductId = i.ProductId,
                        ProductName = string.Empty, // TODO: Obtener del catálogo
                        ProductImageUrl = null,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList(),
                    LastActivityAt = cart.UpdatedAt,
                    AbandonedAt = DateTime.UtcNow
                };

                await publishEndpoint.Publish(cartAbandonedEvent, cancellationToken);

                // Marcar el carrito como notificado para no enviar múltiples notificaciones
                cart.AbandonmentNotifiedAt = DateTime.UtcNow;

                _logger.LogInformation("CartAbandonedEvent published for CartId: {CartId}, ClientId: {ClientId}",
                    cart.CartId, cart.ClientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing CartAbandonedEvent for CartId: {CartId}", cart.CartId);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
