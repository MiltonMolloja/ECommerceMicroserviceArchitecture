using Customer.Domain;
using Customer.Persistence.Database;
using Customer.Service.EventHandlers.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Customer.Service.EventHandlers
{
    public class ClientEventHandler :
        INotificationHandler<ClientCreateCommand>,
        INotificationHandler<ClientUpdateProfileCommand>,
        INotificationHandler<ClientUpdatePreferencesCommand>,
        INotificationHandler<ClientAddressCreateCommand>,
        INotificationHandler<ClientAddressUpdateCommand>,
        INotificationHandler<ClientAddressSetDefaultCommand>,
        INotificationHandler<ClientAddressDeleteCommand>
    {
        private readonly ApplicationDbContext _context;

        public ClientEventHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ClientCreateCommand notification, CancellationToken cancellationToken)
        {
            await _context.AddAsync(new Client
            {
                UserId = notification.UserId,
                FirstName = notification.FirstName,
                LastName = notification.LastName,
                Email = notification.Email,
                Phone = notification.Phone,
                PreferredLanguage = notification.PreferredLanguage ?? "es",
                PreferredCurrency = "USD",
                IsActive = true,
                IsEmailVerified = false,
                NewsletterSubscribed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(ClientUpdateProfileCommand notification, CancellationToken cancellationToken)
        {
            var client = await _context.Clients.FindAsync(notification.ClientId);
            if (client == null) return;

            client.FirstName = notification.FirstName;
            client.LastName = notification.LastName;
            client.Phone = notification.Phone;
            client.MobilePhone = notification.MobilePhone;
            client.DateOfBirth = notification.DateOfBirth;
            client.Gender = notification.Gender;
            client.ProfileImageUrl = notification.ProfileImageUrl;
            client.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(ClientUpdatePreferencesCommand notification, CancellationToken cancellationToken)
        {
            var client = await _context.Clients.FindAsync(notification.ClientId);
            if (client == null) return;

            client.PreferredLanguage = notification.PreferredLanguage;
            client.PreferredCurrency = notification.PreferredCurrency;
            client.NewsletterSubscribed = notification.NewsletterSubscribed;
            client.SmsNotificationsEnabled = notification.SmsNotificationsEnabled;
            client.EmailNotificationsEnabled = notification.EmailNotificationsEnabled;
            client.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(ClientAddressCreateCommand notification, CancellationToken cancellationToken)
        {
            // Si se marca como default, desmarcar las demás
            if (notification.IsDefaultShipping)
            {
                var currentDefaults = await _context.ClientAddresses
                    .Where(x => x.ClientId == notification.ClientId && x.IsDefaultShipping)
                    .ToListAsync(cancellationToken);

                currentDefaults.ForEach(x => x.IsDefaultShipping = false);
            }

            if (notification.IsDefaultBilling)
            {
                var currentDefaults = await _context.ClientAddresses
                    .Where(x => x.ClientId == notification.ClientId && x.IsDefaultBilling)
                    .ToListAsync(cancellationToken);

                currentDefaults.ForEach(x => x.IsDefaultBilling = false);
            }

            await _context.AddAsync(new ClientAddress
            {
                ClientId = notification.ClientId,
                AddressType = notification.AddressType,
                AddressName = notification.AddressName,
                RecipientName = notification.RecipientName,
                RecipientPhone = notification.RecipientPhone,
                AddressLine1 = notification.AddressLine1,
                AddressLine2 = notification.AddressLine2,
                City = notification.City,
                State = notification.State,
                PostalCode = notification.PostalCode,
                Country = notification.Country,
                IsDefaultShipping = notification.IsDefaultShipping,
                IsDefaultBilling = notification.IsDefaultBilling,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(ClientAddressUpdateCommand notification, CancellationToken cancellationToken)
        {
            var address = await _context.ClientAddresses.FindAsync(notification.AddressId);
            if (address == null) return;

            address.AddressName = notification.AddressName;
            address.RecipientName = notification.RecipientName;
            address.RecipientPhone = notification.RecipientPhone;
            address.AddressLine1 = notification.AddressLine1;
            address.AddressLine2 = notification.AddressLine2;
            address.City = notification.City;
            address.State = notification.State;
            address.PostalCode = notification.PostalCode;
            address.Country = notification.Country;
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(ClientAddressSetDefaultCommand notification, CancellationToken cancellationToken)
        {
            var address = await _context.ClientAddresses.FindAsync(notification.AddressId);
            if (address == null) return;

            if (notification.AddressType == "Shipping")
            {
                // Desmarcar otras direcciones de envío
                var currentDefaults = await _context.ClientAddresses
                    .Where(x => x.ClientId == address.ClientId && x.IsDefaultShipping && x.AddressId != address.AddressId)
                    .ToListAsync(cancellationToken);

                currentDefaults.ForEach(x => x.IsDefaultShipping = false);
                address.IsDefaultShipping = true;
            }
            else if (notification.AddressType == "Billing")
            {
                // Desmarcar otras direcciones de facturación
                var currentDefaults = await _context.ClientAddresses
                    .Where(x => x.ClientId == address.ClientId && x.IsDefaultBilling && x.AddressId != address.AddressId)
                    .ToListAsync(cancellationToken);

                currentDefaults.ForEach(x => x.IsDefaultBilling = false);
                address.IsDefaultBilling = true;
            }

            address.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(ClientAddressDeleteCommand notification, CancellationToken cancellationToken)
        {
            var address = await _context.ClientAddresses.FindAsync(notification.AddressId);
            if (address == null) return;

            address.IsActive = false;
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
