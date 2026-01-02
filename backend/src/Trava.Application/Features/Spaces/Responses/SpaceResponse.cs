using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trava.Application.Features.Spaces.Responses
{
    public class SpaceResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string SpaceType { get; set; } = default!;
        public Guid CreatedBy { get; set; }
    }
}