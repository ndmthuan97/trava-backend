using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trava.API.Models;
using Trava.Application.Features.Auth.DTOs;
using Trava.Application.Interfaces.Services;

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
    }
}