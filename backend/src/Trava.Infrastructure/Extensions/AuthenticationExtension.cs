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

                // options.Events = new JwtBearerEvents
                // {
                //     OnTokenValidated = async context =>
                //     {
                //         try
                //         {
                //             var blacklistService = context.HttpContext.RequestServices.GetRequiredService<ITokenBlackListService>();

                //             var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(' ').Last();

                //             if (string.IsNullOrWhiteSpace(token))
                //             {
                //                 context.Fail("Missing access token");
                //                 return;
                //             }

                //             if (await blacklistService.IsTokenBlacklistedAsync(token))
                //             {
                //                 context.Fail("Token has been revoked");
                //                 return;
                //             }

                //             var userId = context.Principal?
                //                 .FindFirstValue(ClaimTypes.NameIdentifier);

                //             if (string.IsNullOrWhiteSpace(userId))
                //             {
                //                 context.Fail("User identifier not found in token");
                //                 return;
                //             }

                //             var issuedAtClaim = context.Principal
                //                 .FindFirst(JwtRegisteredClaimNames.Iat);

                //             if (issuedAtClaim == null ||
                //                 !long.TryParse(issuedAtClaim.Value, out var iat))
                //             {
                //                 context.Fail("Invalid token issued-at claim");
                //                 return;
                //             }

                //             var tokenIssuedAt = DateTimeOffset.FromUnixTimeSeconds(iat).UtcDateTime;

                //             if (await blacklistService.AreUserTokensInvalidatedAsync(userId, tokenIssuedAt))
                //             {
                //                 var exceptionToken = await blacklistService.GetExceptionTokenAsync(userId);

                //                 if (string.IsNullOrEmpty(exceptionToken) || exceptionToken != token)
                //                 {
                //                     context.Fail("User session has been invalidated");
                //                     return;
                //                 }
                //             }
                //         }
                //         catch (Exception)
                //         {
                //             context.Fail("Authentication service unavailable");
                //         }
                //     }
                // };
            });
            return services;
        }
    }
}
