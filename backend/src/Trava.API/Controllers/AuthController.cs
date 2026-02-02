using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trava.API.Models;
using Trava.Application.Features.Auth.Commands;
using Trava.Application.Features.Auth.Responses;
using Trava.Shared.Enums;

namespace Trava.API.Controllers
{
    [ApiController]
    [Route("api/auths")]
    public class AuthController : BaseController<AuthController>
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator, ILogger<AuthController> logger) : base(logger)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            return await HandleRequestAsync(() => _mediator.Send(command));
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            return await HandleRequestAsync(() => _mediator.Send(command));
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var id))
            {
                return Respond(CustomCode.UserIdNotFound);
            }

            var token = Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "").Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                return Respond(CustomCode.AccessTokenInvalidOrExpired);
            }

            // Populate JsonIgnored properties
            command = command with { UserId = id, CurrentAccessToken = token };

            return await HandleRequestAsync(() => _mediator.Send(command));
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            return await HandleRequestAsync(() => _mediator.Send(command));
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

            var command = new LogoutCommand(userId, token);
            return await HandleRequestAsync(async () =>
            {
                await _mediator.Send(command);
            });
        }
    }
}