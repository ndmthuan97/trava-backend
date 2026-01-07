using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Trava.Application.Interfaces.Services;
using Trava.Infrastructure.Services.Identify;
using Trava.Infrastructure.Services.Identify.Interfaces;

namespace Trava.Infrastructure.Extensions
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<JwtHandler>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenBlackListService, TokenBlackListService>();

            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = jwtSettings.ValidIssuer,
                    ValidAudience = jwtSettings.ValidAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
                    ),

                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    // Add custom token validation for blacklisted tokens
                    OnTokenValidated = async context =>
                    {
                        var tokenBlacklistService = context.HttpContext.RequestServices
                            .GetRequiredService<ITokenBlackListService>();

                        var token = context.Request.Headers["Authorization"]
                            .FirstOrDefault()?.Split(" ").Last();

                        if (string.IsNullOrEmpty(token))
                        {
                            return;
                        }

                        // Check if individual token is blacklisted
                        if (!string.IsNullOrEmpty(token) && await tokenBlacklistService.IsTokenBlacklistedAsync(token))
                        {
                            context.Fail("Token has been revoked");
                            return;
                        }

                        // Check if all user tokens have been invalidated
                        var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (!string.IsNullOrEmpty(userIdClaim))
                        {
                            var tokenHandler = new JwtSecurityTokenHandler();
                            var jwtToken = tokenHandler.ReadJwtToken(token);
                            var tokenIssuedAt = jwtToken.IssuedAt; // Use DateTime directly

                            if (await tokenBlacklistService.AreUserTokensInvalidatedAsync(userIdClaim, tokenIssuedAt))
                            {
                                var exceptionToken = await tokenBlacklistService.GetExceptionTokenAsync(userIdClaim);

                                if (!string.IsNullOrEmpty(exceptionToken))
                                {
                                    if (token != exceptionToken)
                                    {
                                        context.Fail("This session was logged out by user.");
                                        return;
                                    }
                                }
                                else
                                {
                                    context.Fail("All user tokens have been invalidated");
                                    return;
                                }
                            }
                        }
                    }
                };
            });
            return services;
        }
    }
}
