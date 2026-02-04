using System;

namespace Trava.Application.Features.Spaces.Responses
{
    public record SpaceResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
        public string SpaceType { get; init; } = default!;
        public Guid CreatedBy { get; init; }
        public int CountMember { get; init; }
    }
}