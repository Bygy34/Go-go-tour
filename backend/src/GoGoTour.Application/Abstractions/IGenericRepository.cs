using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GoGoTour.Application.Abstractions
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken));
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default(CancellationToken));
    }
}
