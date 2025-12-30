using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trava.Shared.Constants;
using Trava.Shared.Enums;

namespace Trava.Application.Common.Exceptions
{
    public class AppException : Exception
    {
        public CustomCode StatusCode { get; }
        public IEnumerable<string>? Errors { get; }

        public AppException(CustomCode statusCode, IEnumerable<string>? errors = null)
            : base(ResponseMessages.Messages.GetValueOrDefault(statusCode)?.Message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}