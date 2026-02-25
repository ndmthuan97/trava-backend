using System;

namespace Trava.Application.Features.TaskItems.Responses
{
    public record TaskCommentResponse
    {
        public Guid Id { get; init; }
        public Guid TaskItemId { get; init; }
        public Guid UserId { get; init; }
        public string? FullName { get; init; }
        public string? AvatarUrl { get; init; }
        public string Content { get; init; } = default!;
        public DateTimeOffset CreatedAt { get; init; }
    }
}
