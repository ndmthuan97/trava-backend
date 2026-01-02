using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trava.API.Models;
using Trava.Application.Features.TaskItems.Commands;
using Trava.Domain.Enums;
using Trava.Shared.Enums;

namespace Trava.API.Controllers
{
    [ApiController]
    [Route("api/taskitems")]
    public class TaskItemController : BaseController<TaskItemController>
    {
        private readonly IMediator _mediator;
        public TaskItemController(IMediator mediator, ILogger<TaskItemController> logger) : base(logger)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(Role.SystemAdmin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CompleteTaskItem([FromBody] CreateTaskItemCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            command.CreatedBy = userIdGuid;
            return await HandleRequestAsync(() => _mediator.Send(command), CustomCode.Created);
        }
        
        [HttpPut("{id:guid}")]
        [Authorize(Roles = $"{nameof(Role.SystemAdmin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTaskItem([FromRoute] Guid id, [FromBody] UpdateTaskItemCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            command.Id = id;
            return await HandleRequestAsync(() => _mediator.Send(command), CustomCode.Updated);
        }

        [HttpPut("complete/{id:guid}")]
        [Authorize(Roles = $"{nameof(Role.SystemAdmin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CompleteTaskItem([FromBody] CompleteTaskItemCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            command.CompletedBy = userIdGuid;
            return await HandleRequestAsync(() => _mediator.Send(command), CustomCode.Updated);
        }
    }
}