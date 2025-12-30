using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trava.Application.Interfaces.Repositories;

namespace Trava.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : class;

        TRepository GetCustomRepository<TRepository>()
            where TRepository : class;

        Task<int> CommitAsync();
    }
}