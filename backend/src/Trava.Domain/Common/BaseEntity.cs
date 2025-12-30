using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trava.Domain.Common
{
    public abstract class BaseEntity<TKey>
    {
        [Key]
        public TKey Id { get; set; } = default!;
    }
}
