using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trava.API.Models;
using Trava.Application.Features.Spaces.Commands;
using Trava.Domain.Enums;
using Trava.Shared.Enums;

namespace Trava.API.Controllers
{
    [ApiController]
    [Route("api/spaces")]
    public class SpaceController : BaseController<SpaceController>
    {
        private readonly IMediator _mediator;
        public SpaceController(IMediator mediator, ILogger<SpaceController> logger) : base(logger)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(Role.SystemAdmin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateSpace([FromBody] CreateSpaceCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            command.CreatedBy = userIdGuid;
            return await HandleRequestAsync(() => _mediator.Send(command), CustomCode.Created);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSpace([FromRoute] Guid id, [FromBody] UpdateSpaceCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            command.Id = id;
            return await HandleRequestAsync(() => _mediator.Send(command), CustomCode.Updated);
        }
    }
}