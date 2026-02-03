using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trava.API.Models;
using Trava.Application.Common.Models;
using Trava.Application.Features.Users.Commands;
using Trava.Application.Features.Users.Queries;
using Trava.Application.Features.Users.Responses;
using Trava.Application.Features.Users.Specifications;
using Trava.Domain.Enums;
using Trava.Shared.Enums;

namespace Trava.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : BaseController<UserController>
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator, ILogger<UserController> logger) : base(logger)
        {
            _mediator = mediator;
        }

        [HttpGet()]
        [Authorize(Roles = nameof(Role.Admin))]
        [ProducesResponseType(typeof(Pagination<UserResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers([FromQuery] UserSpecParam param)
        {
            return await HandleRequestAsync( async () =>
            {
                return (CustomCode.Success, await _mediator.Send(new GetUsersQuery(param)));
            });
        }

        [HttpGet("profile")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                return Respond(CustomCode.UserIdNotFound);
            }

            return await HandleRequestAsync(async () =>
            {
                return (CustomCode.Success, await _mediator.Send(new GetUserProfileQuery(userIdGuid)));
            });
        }

        [HttpGet("profile/{id:guid}")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfileById([FromRoute] Guid id)
        {
            return await HandleRequestAsync(async () =>
            {
                return (CustomCode.Success, await _mediator.Send(new GetUserProfileByIdQuery(id)));
            });
        }

        [HttpPut("status/{id:guid}")]
        [Authorize(Roles = nameof(Role.Admin))]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateUserStatus([FromRoute] Guid id, [FromBody] UpdateUserStatusCommand command)
        {
            return await HandleRequestAsync(() => _mediator.Send(command with { Id = id }), CustomCode.Updated);
        }

        [HttpPut("profile")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                return Respond(CustomCode.UserIdNotFound);
            }

            return await HandleRequestAsync(() => _mediator.Send(command with { Id = userIdGuid }), CustomCode.Updated);
        }
    }
}
