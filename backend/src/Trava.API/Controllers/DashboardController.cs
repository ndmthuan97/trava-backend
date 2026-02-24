using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Trava.API.Models;
using Trava.Application.Features.Dashboards.Queries;
using Trava.Application.Features.Dashboards.Responses;
using Trava.Domain.Enums;
using Trava.Shared.Enums;

namespace Trava.API.Controllers
{
    [ApiController]
    [Route("api/dashboards")]
    public class DashboardController : BaseController<DashboardController>
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator, ILogger<DashboardController> logger) : base(logger)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = nameof(Role.Admin))]
        [ProducesResponseType(typeof(ApiResponse<StatisticsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdminDashboard()
        {
            return await HandleRequestAsync(async () =>
            {
                return (CustomCode.Success, await _mediator.Send(new GetStatisticsQuery()));
            });
        }
    }
}
