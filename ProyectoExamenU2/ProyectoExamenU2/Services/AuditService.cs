using ProyectoExamenU2.Constants;
using ProyectoExamenU2.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProyectoExamenU2.Services
{
    public class AuditService:IAuditService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public AuditService(
            IHttpContextAccessor httpContextAccessor,
            TokenValidationParameters tokenValidationParameters)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenValidationParameters = tokenValidationParameters;
        }

        public string GetUserId()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var claimsPrincipal = handler.ValidateToken(token, _tokenValidationParameters, out _);
                var userIdClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "UserId");

                return userIdClaim?.Value;
            }
            catch (Exception)
            {
                // Opcional: Log o manejo de errores de validación del token.
                return null;
            }
        }

    }
}
