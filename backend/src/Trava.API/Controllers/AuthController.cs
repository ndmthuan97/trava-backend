using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trava.API.Models;
using Trava.Application.Features.Auth.DTOs;
using Trava.Application.Interfaces.Services;
using Trava.Shared.Enums;

namespace Trava.API.Controllers
{
    [ApiController]
    [Route("api/auths")]
    public class AuthController : BaseController<AuthController>
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService, ILogger<AuthController> logger) : base(logger)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            return await HandleRequestAsync(() => _authService.RegisterAsync(request));
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            return await HandleRequestAsync(() => _authService.LoginAsync(request));
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var id))
            {
                return Respond(CustomCode.UserIdNotFound);
            }

            dto.UserId = id;
            var token = Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "").Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                return Respond(CustomCode.AccessTokenInvalidOrExpired);
            }

            dto.CurrentAccessToken = token;
            return await HandleRequestAsync(() => _authService.ChangePasswordAsync(dto));
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Respond(CustomCode.UserIdNotFound);
            }
            var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            return await HandleRequestAsync(() => _authService.LogoutAsync(userId, token));
        }
    }
}