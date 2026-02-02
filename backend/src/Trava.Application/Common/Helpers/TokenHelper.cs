using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Trava.Domain.Entities;

namespace Trava.Application.Common.Helpers
{
    public static class TokenHelper
    {
        public static List<Claim> GetClaims(User user, IList<Claim> userClaims)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            claims.AddRange(userClaims);
            return claims;
        }

        public static string GenerateRefreshToken()
        {
            var randomNumber = new Byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static DateTime GetTokenExpiry(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
    }
}
