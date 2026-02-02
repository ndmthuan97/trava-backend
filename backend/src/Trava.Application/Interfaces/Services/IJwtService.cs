using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Trava.Application.Interfaces.Services
{
    public interface IJwtService
    {
        JwtSecurityToken GenerateTokenOptions(IEnumerable<Claim> claims);
        int GetExpiryInSecond();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
