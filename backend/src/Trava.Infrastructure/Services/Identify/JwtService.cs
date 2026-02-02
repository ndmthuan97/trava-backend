using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Trava.Application.Common.Exceptions;
using Trava.Shared.Enums;

using Trava.Application.Interfaces.Services;

namespace Trava.Infrastructure.Services.Identify
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = default!;
        public string ValidIssuer { get; set; } = default!;
        public string ValidAudience { get; set; } = default!;
        public int ExpiryInSecond { get; set; }
    }

    public sealed class JwtService : IJwtService
    {
        private readonly JwtSettings _settings;
        private readonly ILogger<JwtService> _logger;
        private readonly SymmetricSecurityKey _signingKey;

        public JwtService(IOptions<JwtSettings> options, ILogger<JwtService> logger)
        {
            _settings = options.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
                throw new InvalidOperationException("JWT SecretKey is missing");

            _signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_settings.SecretKey)
            );
        }

        public SigningCredentials GetSigningCredentials()
            => new(_signingKey, SecurityAlgorithms.HmacSha256);

        public JwtSecurityToken GenerateTokenOptions(IEnumerable<Claim> claims)
        {
            var now = DateTime.UtcNow;
            var tokenClaims = new List<Claim>(claims)
    {
        new(JwtRegisteredClaimNames.Iat,
            new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
            ClaimValueTypes.Integer64)
    };

            return new JwtSecurityToken(
                issuer: _settings.ValidIssuer,
                audience: _settings.ValidAudience,
                claims: tokenClaims,
                notBefore: now,
                expires: now.AddSeconds(_settings.ExpiryInSecond),
                signingCredentials: GetSigningCredentials()
            );
        }

        public int GetExpiryInSecond() => _settings.ExpiryInSecond;

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, parameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwt ||
                    jwt.Header.Alg != SecurityAlgorithms.HmacSha256)
                {
                    throw new SecurityTokenException("Invalid token algorithm");
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid refresh token");
                throw new AppException(CustomCode.InvalidToken, new[] { "Refresh token is invalid or expired" });
            }
        }
    }
}