using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trava.Application.Interfaces.Repositories
{
    public interface IFactoryRepository
    {
        IGenericRepository<TEntity, TKey> GetGenericRepository<TEntity, TKey>()
            where TEntity : class;

        TRepository GetCustomRepository<TRepository>()
            where TRepository : class;
    }
}