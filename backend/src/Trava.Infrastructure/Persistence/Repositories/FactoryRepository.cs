using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Trava.Application.Interfaces.Repositories;
using Trava.Infrastructure.Persistence.Context;

namespace Trava.Infrastructure.Persistence.Repositories
{
    public class FactoryRepository : IFactoryRepository
    {
        private readonly AppDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public FactoryRepository(AppDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        public IGenericRepository<TEntity, TKey> GetGenericRepository<TEntity, TKey>()
            where TEntity : class
        {
            return new GenericRepository<TEntity, TKey>(_context);
        }

        public TRepository GetCustomRepository<TRepository>()
            where TRepository : class
        {
            return _serviceProvider.GetService<TRepository>()
                ?? throw new InvalidOperationException($"Repository {typeof(TRepository).Name} not registered");
        }
    }
}