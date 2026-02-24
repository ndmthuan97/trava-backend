using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trava.API.Models;
using Trava.Application.Features.SpaceInvitations.Commands;
using Trava.Application.Features.SpaceInvitations.Queries;
using Trava.Application.Features.SpaceInvitations.Responses;
using Trava.Application.Common.Models;
using Trava.Application.Features.SpaceInvitations.Specifications;
using Trava.Domain.Enums;
using Trava.Shared.Enums;

namespace Trava.API.Controllers
{
    [ApiController]
    [Route("api/invitations")]
    public class SpaceInvitationController : BaseController<SpaceInvitationController>
    {
        private readonly IMediator _mediator;
        public SpaceInvitationController(IMediator mediator, ILogger<SpaceInvitationController> logger) : base(logger)
        {
            _mediator = mediator;
        }

        [HttpGet("my-invitations")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<SpaceInvitationResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyInvitations([FromQuery] InvitationSpecParam param)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            param.InvitedUserId = userIdGuid;
            return await HandleRequestAsync(async () => (CustomCode.Success, await _mediator.Send(new GetMyInvitationsQuery(param))));
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateSpaceInvitation([FromBody] CreateSpaceInvitationCommand command)
        {
            return await HandleRequestAsync(async () => (CustomCode.Created, await _mediator.Send(command)));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStatusSpaceInvitation([FromRoute] Guid id, [FromBody] UpdateStatusSpaceInvitationCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            command = command with { Id = id, InvitatedUser = userIdGuid };
            return await HandleRequestAsync(() => _mediator.Send(command), CustomCode.Updated);
        }
    }
}