using Customer.Persistence.Database;
using Customer.Service.Queries.DTOs;
using Microsoft.EntityFrameworkCore;
using Service.Common.Collection;
using Service.Common.Mapping;
using Service.Common.Paging;
using System;
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
                // NOTA: No podemos ordenar por FirstName/LastName ya que no están en esta tabla
                .OrderBy(x => x.ClientId)
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
                // NOTA: No podemos ordenar por FirstName/LastName ya que no están en esta tabla
                .OrderBy(x => x.ClientId)
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
        /// OBSOLETO: Email ya no está en esta tabla. Usar GetByUserIdAsync en su lugar.
        /// </summary>
        [Obsolete("Email field removed from Client entity. Use GetByUserIdAsync instead.")]
        public async Task<ClientDto> GetByEmailAsync(string email)
        {
            // Este método ya no es funcional porque Email se eliminó de la tabla Client
            // Se debe usar GetByUserIdAsync y obtener el UserId desde Identity
            return null;
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
                // NOTA: FirstName, LastName, Email deben obtenerse de Identity.AspNetUsers
                // Por ahora se dejan null - el frontend puede obtenerlos del JWT o llamando a Identity.Api
                FirstName = null,
                LastName = null,
                FullName = "From Identity",
                DisplayName = "From Identity",
                Email = null,
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
