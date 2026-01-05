using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trava.API.Models;
using Trava.Application.Common.Exceptions;
using Trava.Shared.Constants;
using Trava.Shared.Enums;

namespace Trava.API.Controllers
{
    [ApiController]
    public abstract class BaseController<TController> : ControllerBase
    {
        protected readonly ILogger<BaseController<TController>> _logger;
        public BaseController(ILogger<BaseController<TController>> logger)
        {
            _logger = logger;
        }

        protected IActionResult CheckModelStateValidity()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                _logger.LogWarning("Request validation failed for {ControllerName}: {ErrorMessages}", typeof(TController).Name, string.Join(", ", errors));

                return Respond(CustomCode.ModelInvalid, null, errors);
            }
            return null!;
        }

        protected IActionResult Respond(CustomCode code, object? data = null, IEnumerable<string>? errors = null)
        {
            ResponseMessages.Messages.TryGetValue(code, out var msgDetail);

            var responseData = new ApiResponse<object>
            {
                StatusCode = (int)code,
                Message = msgDetail?.Message ?? "Unknown error",
                Data = data,
                Errors = errors
            };

            return StatusCode(msgDetail?.HttpCode ?? StatusCodes.Status500InternalServerError, responseData);
        }

        private async Task<IActionResult> ExecuteAsync<T>(
            Func<Task<(CustomCode code, T result)>> func,
            Func<T, object?> resultSelector,
            T defaultResult,
            CustomCode? overrideSuccessCode = null)
        {
            var modelCheck = CheckModelStateValidity();
            if (modelCheck != null)
                return modelCheck;

            try
            {
                var (code, result) = await func();
                return Respond(overrideSuccessCode ?? code, resultSelector(result));
            }
            catch (AppException ex)
            {
                var errors = ex.Errors ?? new[] { ex.Message };

                return Respond(ex.StatusCode, resultSelector(defaultResult), errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in {ControllerName}", typeof(TController).Name);
                return Respond(CustomCode.SystemError, null, new[] { ex.Message });
            }
        }

        protected Task<IActionResult> HandleRequestAsync<TResponse>(Func<Task<(CustomCode code, TResponse result)>> func)
            where TResponse : class => ExecuteAsync(func, result => result, default(TResponse)!);

        // No result (void flow)
        protected Task<IActionResult> HandleRequestAsync(Func<Task> func, CustomCode successCode = CustomCode.Success)
            => ExecuteAsync<object?>(
                async () =>
                {
                    await func();
                    return (successCode, (object?)null);
                },
                _ => null,
                null);
    }
}