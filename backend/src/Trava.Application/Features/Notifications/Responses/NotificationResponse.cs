using System;

namespace Trava.Application.Features.Notifications.Responses
{
    public record NotificationResponse
    {
        public Guid Id { get; init; }
        public string Type { get; init; } = default!;
        public object Payload { get; init; } = default!;
        public DateTimeOffset CreatedAt { get; init; }
        public bool IsRead { get; init; }
    }
}
