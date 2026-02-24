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
using Trava.Application.Features.TaskItems.Queries;
using Trava.Application.Features.TaskItems.Specifications;

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

        [HttpGet]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTaskItems([FromQuery] TaskItemSpecParam param)
        {
            return await HandleRequestAsync(async () =>
            {
                return (CustomCode.Success, await _mediator.Send(new GetTaskItemsBySpaceQuery(param)));
            });
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTaskItemById([FromRoute] Guid id)
        {
            return await HandleRequestAsync(async () =>
            {
                return (CustomCode.Success, await _mediator.Send(new GetTaskItemByIdQuery(id)));
            });
        }

        [HttpGet("my-tasks")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyTasks([FromQuery] TaskItemSpecParam param)
        {
            return await HandleRequestAsync(async () =>
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userId, out var userIdGuid))
                    return (CustomCode.UserIdNotFound, null);

                param.AssignedUserId = userIdGuid;
                return (CustomCode.Success, await _mediator.Send(new GetTaskItemsBySpaceQuery(param)));
            });
        }

        [HttpGet("spaces")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTasksBySpace([FromQuery] TaskItemSpecParam param)
        {
            return await HandleRequestAsync(async () =>
            {
                return (CustomCode.Success, await _mediator.Send(new GetTaskItemsBySpaceQuery(param)));
            });
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CompleteTaskItem([FromBody] CreateTaskItemCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            command = command with { CreatedBy = userIdGuid };
            return await HandleRequestAsync(async () => (CustomCode.Created, await _mediator.Send(command)));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTaskItem([FromRoute] Guid id, [FromBody] UpdateTaskItemCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            command = command with { Id = id, UpdatedBy = userIdGuid };
            return await HandleRequestAsync(() => _mediator.Send(command), CustomCode.Updated);
        }

        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTaskStatus([FromRoute] Guid id, [FromBody] UpdateTaskStatusCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            command = command with { Id = id, UserId = userIdGuid };
            return await HandleRequestAsync(() => _mediator.Send(command), CustomCode.Updated);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteTaskItem([FromRoute] Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            var command = new DeleteTaskItemCommand(id, userIdGuid);
            return await HandleRequestAsync(() => _mediator.Send(command), CustomCode.Success);
        }

        [HttpGet("{id:guid}/comments")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        public async Task<IActionResult> GetTaskComments([FromRoute] Guid id)
        {
            return await HandleRequestAsync(async () =>
            {
                return (CustomCode.Success, await _mediator.Send(new GetTaskCommentsQuery(id)));
            });
        }

        [HttpPost("{id:guid}/comments")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.User)}")]
        public async Task<IActionResult> CreateTaskComment([FromRoute] Guid id, [FromBody] CreateTaskCommentCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            return await HandleRequestAsync(async () =>
            {
                return (CustomCode.Created, await _mediator.Send(command with { TaskItemId = id, UserId = userIdGuid }));
            });
        }
    }
}