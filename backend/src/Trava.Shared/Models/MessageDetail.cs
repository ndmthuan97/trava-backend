using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trava.Shared.Models
{
    public class MessageDetail
    {
        public int HttpCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}