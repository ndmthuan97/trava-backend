using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trava.Domain.Common
{
    public abstract class BaseTimeEntity<TKey> : BaseEntity<TKey>
    {
        public Guid CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}