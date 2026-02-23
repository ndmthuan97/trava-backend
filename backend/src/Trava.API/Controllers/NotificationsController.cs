using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trava.API.Models;
using Trava.Application.Common.Models;
using Trava.Application.Features.Notifications.Queries;
using Trava.Application.Features.Notifications.Responses;
using Trava.Application.Features.Notifications.Commands;
using Trava.Shared.Enums;

namespace Trava.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : BaseController<NotificationsController>
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator, ILogger<NotificationsController> logger) : base(logger)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Pagination<NotificationResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotifications([FromQuery] GetUserNotificationsQuery query)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            query.UserId = userIdGuid;
            return await HandleRequestAsync(async () => (CustomCode.Success, await _mediator.Send(query)));
        }

        [HttpGet("unread")]
        [ProducesResponseType(typeof(ApiResponse<List<NotificationResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            var query = new GetUnreadNotificationsQuery { UserId = userIdGuid };
            return await HandleRequestAsync(async () => (CustomCode.Success, await _mediator.Send(query)));
        }

        [HttpPut("read/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAsRead([FromRoute] Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            var command = new MarkAsReadCommand(id, userIdGuid);
            return await HandleRequestAsync(async () =>
            {
                var result = await _mediator.Send(command);
                return (CustomCode.Updated, (object)result);
            });
        }

        [HttpPut("read-all")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Respond(CustomCode.UserIdNotFound);

            var command = new MarkAllAsReadCommand(userIdGuid);
            return await HandleRequestAsync(async () =>
            {
                var result = await _mediator.Send(command);
                return (CustomCode.Updated, (object)result);
            });
        }
    }
}
