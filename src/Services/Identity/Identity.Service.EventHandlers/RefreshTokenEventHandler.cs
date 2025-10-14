using Identity.Domain;
using Identity.Persistence.Database;
using Identity.Service.EventHandlers.Commands;
using Identity.Service.EventHandlers.Responses;
using Identity.Service.EventHandlers.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers
{
    public class RefreshTokenEventHandler : IRequestHandler<RefreshTokenCommand, IdentityAccess>
    {
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public RefreshTokenEventHandler(
            IRefreshTokenService refreshTokenService,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _refreshTokenService = refreshTokenService;
            _context = context;
            _configuration = configuration;
        }

        public async Task<IdentityAccess> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var result = new IdentityAccess();

            try
            {
                // Validar el refresh token
                var refreshToken = await _refreshTokenService.ValidateRefreshTokenAsync(request.RefreshToken);

                // Obtener el usuario
                var user = refreshToken.User;

                // Generar nuevo access token
                result.Succeeded = true;
                await GenerateToken(user, result);

                // Generar nuevo refresh token
                var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, request.IpAddress);

                // Revocar el refresh token antiguo
                await _refreshTokenService.RevokeRefreshTokenAsync(
                    request.RefreshToken,
                    request.IpAddress,
                    newRefreshToken.Token
                );

                result.RefreshToken = newRefreshToken.Token;
                result.ExpiresAt = newRefreshToken.ExpiresAt;
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                // Log the error here if needed
            }

            return result;
        }

        private async Task GenerateToken(ApplicationUser user, IdentityAccess identity)
        {
            var secretKey = _configuration.GetValue<string>("SecretKey");
            var key = Encoding.ASCII.GetBytes(secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName)
            };

            var roles = await _context.Roles
                                      .Where(x => x.UserRoles.Any(y => y.UserId == user.Id))
                                      .ToListAsync();

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var createdToken = tokenHandler.CreateToken(tokenDescriptor);

            identity.AccessToken = tokenHandler.WriteToken(createdToken);
        }
    }
}
