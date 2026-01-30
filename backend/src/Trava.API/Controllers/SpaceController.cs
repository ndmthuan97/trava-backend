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
using Trava.Application.Features.Spaces.Queries;
using Trava.Application.Features.Spaces.Responses;
using Trava.Application.Features.Spaces.Specifications;
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

        [HttpGet]
        [Authorize(Roles = $"{nameof(Role.SystemAdmin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSpaces([FromQuery] SpaceSpecParam param)
        {
            return await HandleRequestAsync(async () =>
            {
                return (CustomCode.Success, await _mediator.Send(new GetSpacesQuery(param)));
            });
        }

        [HttpGet("my-spaces")]
        [Authorize(Roles = $"{nameof(Role.SystemAdmin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<List<SpaceResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMySpaces()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            return await HandleRequestAsync(async () =>
            {
                return (CustomCode.Success, await _mediator.Send(new GetSpacesByUserQuery(userIdGuid)));
            });
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = $"{nameof(Role.SystemAdmin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSpaceById([FromRoute] Guid id)
        {
            return await HandleRequestAsync(async () =>
            {
                return (CustomCode.Success, await _mediator.Send(new GetSpaceByIdQuery(id)));
            });
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(Role.SystemAdmin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateSpace([FromBody] CreateSpaceCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            command = command with { CreatedBy = userIdGuid };
            return await HandleRequestAsync(async () =>
                (CustomCode.Created, await _mediator.Send(command with { CreatedBy = userIdGuid }))
            );
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSpace([FromRoute] Guid id, [FromBody] UpdateSpaceCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            return await HandleRequestAsync(() => _mediator.Send(command with { Id = id }), CustomCode.Updated);
        }
    }
}