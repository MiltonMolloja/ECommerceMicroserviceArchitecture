using Identity.Service.EventHandlers.Commands;
using Identity.Service.EventHandlers.Services;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers
{
    public class RevokeTokenEventHandler : IRequestHandler<RevokeTokenCommand, bool>
    {
        private readonly IRefreshTokenService _refreshTokenService;

        public RevokeTokenEventHandler(IRefreshTokenService refreshTokenService)
        {
            _refreshTokenService = refreshTokenService;
        }

        public async Task<bool> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken, request.IpAddress);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
