using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trava.Domain.Entities;

namespace Trava.Application.Interfaces.Repositories
{
    public interface IUserRpository : IGenericRepository<User, Guid>
    {

    }
}