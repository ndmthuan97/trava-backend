using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Trava.Application.Interfaces;
using Trava.Application.Interfaces.Repositories;
using Trava.Infrastructure.Persistence.Context;
using Trava.Infrastructure.Persistence.Repositories;

namespace Trava.Infrastructure.Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        private readonly Dictionary<string, object> _repositories = new();

        public UnitOfWork(AppDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : class
        {
            var key = typeof(TEntity).Name;

            if (!_repositories.ContainsKey(key))
            {
                var repo = new GenericRepository<TEntity, TKey>(_context);
                _repositories[key] = repo;
            }

            return (IGenericRepository<TEntity, TKey>)_repositories[key];
        }

        public TRepository GetCustomRepository<TRepository>()
            where TRepository : class
        {
            return _serviceProvider.GetRequiredService<TRepository>();
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}