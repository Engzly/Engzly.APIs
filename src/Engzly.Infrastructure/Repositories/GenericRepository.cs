using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Interfaces.Specifications;
using Engzly.Infrastructure.Persistence.Data;
using Engzly.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Engzly.Infrastructure.Repositories
{
    public class GenericRepository<T>(EngzlyDbContext context) : IGenericRepository<T>
        where T : class
    {
        private readonly DbContext _context = context;

        public async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
        {
            var query = SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
        {
            var query = SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);
            return await query.ToListAsync(cancellationToken);
        }
    }
}
