using System;

namespace Trava.Application.Features.Spaces.Responses
{
    public record SpaceResponse(
        Guid Id,
        string Name,
        string Description,
        string SpaceType,
        Guid CreatedBy
    );
}