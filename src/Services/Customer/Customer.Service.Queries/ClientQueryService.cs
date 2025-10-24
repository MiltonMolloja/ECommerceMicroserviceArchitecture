using Customer.Persistence.Database;
using Customer.Service.Queries.DTOs;
using Microsoft.EntityFrameworkCore;
using Service.Common.Collection;
using Service.Common.Mapping;
using Service.Common.Paging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Service.Queries
{
    public interface IClientQueryService
    {
        Task<DataCollection<ClientDto>> GetAllAsync(int page, int take, IEnumerable<int> clients = null);
        Task<DataCollection<ClientSummaryDto>> GetAllSummaryAsync(int page, int take);
        Task<ClientDto> GetAsync(int id);
        Task<ClientDto> GetByEmailAsync(string email);
        Task<ClientDto> GetByUserIdAsync(string userId);
        Task<List<ClientAddressDto>> GetAddressesAsync(int clientId);
        Task<ClientAddressDto> GetAddressAsync(int addressId);
    }

    public class ClientQueryService : IClientQueryService
    {
        private readonly ApplicationDbContext _context;

        public ClientQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los clientes con información completa (incluyendo direcciones)
        /// </summary>
        public async Task<DataCollection<ClientDto>> GetAllAsync(int page, int take, IEnumerable<int> clients = null)
        {
            var collection = await _context.Clients
                .Include(x => x.Addresses)
                .Where(x => clients == null || clients.Contains(x.ClientId))
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .GetPagedAsync(page, take);

            var dtos = collection.Items.Select(MapToCompleteDto).ToList();

            return new DataCollection<ClientDto>
            {
                Items = dtos,
                Total = collection.Total,
                Page = collection.Page,
                Pages = collection.Pages
            };
        }

        /// <summary>
        /// Obtiene un resumen de clientes (para listados, más ligero)
        /// </summary>
        public async Task<DataCollection<ClientSummaryDto>> GetAllSummaryAsync(int page, int take)
        {
            var collection = await _context.Clients
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .GetPagedAsync(page, take);

            return collection.MapTo<DataCollection<ClientSummaryDto>>();
        }

        /// <summary>
        /// Obtiene un cliente por ID con información completa
        /// </summary>
        public async Task<ClientDto> GetAsync(int id)
        {
            var client = await _context.Clients
                .Include(x => x.Addresses)
                .FirstOrDefaultAsync(x => x.ClientId == id);

            if (client == null) return null;

            return MapToCompleteDto(client);
        }

        /// <summary>
        /// Obtiene un cliente por email
        /// </summary>
        public async Task<ClientDto> GetByEmailAsync(string email)
        {
            var client = await _context.Clients
                .Include(x => x.Addresses)
                .FirstOrDefaultAsync(x => x.Email == email);

            if (client == null) return null;

            return MapToCompleteDto(client);
        }

        /// <summary>
        /// Obtiene un cliente por UserId (del sistema de identidad)
        /// </summary>
        public async Task<ClientDto> GetByUserIdAsync(string userId)
        {
            var client = await _context.Clients
                .Include(x => x.Addresses)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (client == null) return null;

            return MapToCompleteDto(client);
        }

        /// <summary>
        /// Obtiene todas las direcciones de un cliente
        /// </summary>
        public async Task<List<ClientAddressDto>> GetAddressesAsync(int clientId)
        {
            var addresses = await _context.ClientAddresses
                .Where(x => x.ClientId == clientId && x.IsActive)
                .OrderByDescending(x => x.IsDefaultShipping)
                .ThenByDescending(x => x.IsDefaultBilling)
                .ThenBy(x => x.AddressName)
                .ToListAsync();

            return addresses.MapTo<List<ClientAddressDto>>();
        }

        /// <summary>
        /// Obtiene una dirección específica
        /// </summary>
        public async Task<ClientAddressDto> GetAddressAsync(int addressId)
        {
            var address = await _context.ClientAddresses
                .FirstOrDefaultAsync(x => x.AddressId == addressId);

            return address?.MapTo<ClientAddressDto>();
        }

        #region Private Methods

        /// <summary>
        /// Mapea una entidad Client a ClientDto completo con todas las propiedades calculadas
        /// </summary>
        private ClientDto MapToCompleteDto(Customer.Domain.Client client)
        {
            var dto = new ClientDto
            {
                ClientId = client.ClientId,
                UserId = client.UserId,
                FirstName = client.FirstName,
                LastName = client.LastName,
                FullName = client.FullName,
                DisplayName = client.DisplayName,
                Email = client.Email,
                Phone = client.Phone,
                MobilePhone = client.MobilePhone,
                DateOfBirth = client.DateOfBirth,
                Age = client.Age,
                Gender = client.Gender,
                ProfileImageUrl = client.ProfileImageUrl,
                PreferredLanguage = client.PreferredLanguage,
                PreferredCurrency = client.PreferredCurrency,
                NewsletterSubscribed = client.NewsletterSubscribed,
                SmsNotificationsEnabled = client.SmsNotificationsEnabled,
                EmailNotificationsEnabled = client.EmailNotificationsEnabled,
                IsActive = client.IsActive,
                IsEmailVerified = client.IsEmailVerified,
                IsPhoneVerified = client.IsPhoneVerified,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt,
                LastLoginAt = client.LastLoginAt
            };

            // Mapear direcciones SIN incluir la referencia circular de Client
            if (client.Addresses != null && client.Addresses.Any())
            {
                dto.Addresses = client.Addresses.Select(a => new ClientAddressDto
                {
                    AddressId = a.AddressId,
                    ClientId = a.ClientId,
                    AddressType = a.AddressType,
                    AddressName = a.AddressName,
                    RecipientName = a.RecipientName,
                    RecipientPhone = a.RecipientPhone,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    City = a.City,
                    State = a.State,
                    PostalCode = a.PostalCode,
                    Country = a.Country,
                    FullAddress = a.FullAddress,
                    ShortAddress = a.ShortAddress,
                    IsDefaultShipping = a.IsDefaultShipping,
                    IsDefaultBilling = a.IsDefaultBilling,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).ToList();

                // Encontrar direcciones predeterminadas
                dto.DefaultShippingAddress = dto.Addresses.FirstOrDefault(a => a.IsDefaultShipping);
                dto.DefaultBillingAddress = dto.Addresses.FirstOrDefault(a => a.IsDefaultBilling);
            }
            else
            {
                dto.Addresses = new List<ClientAddressDto>();
            }

            return dto;
        }

        #endregion
    }
}
