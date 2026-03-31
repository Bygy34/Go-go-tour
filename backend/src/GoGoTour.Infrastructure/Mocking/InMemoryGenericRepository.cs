using GoGoTour.Application.Abstractions;
using GoGoTour.Domain.Entities;

namespace GoGoTour.Infrastructure.Mocking;

public class InMemoryGenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly InMemoryStore _store;

    public InMemoryGenericRepository(InMemoryStore store)
    {
        _store = store;
    }

    private List<T> Data => typeof(T) switch
    {
        var x when x == typeof(Tour) => (List<T>)(object)_store.Tours,
        var x when x == typeof(BookingRequest) => (List<T>)(object)_store.BookingRequests,
        _ => throw new InvalidOperationException($"Type {typeof(T).Name} is not supported by in-memory store")
    };

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<T>>(Data.ToList());
    }

    public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = Data.FirstOrDefault(x => GetId(x) == id);
        return Task.FromResult(entity);
    }

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var nextId = Data.Count == 0 ? 1 : Data.Max(GetId) + 1;
        SetId(entity, nextId);
        Data.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var id = GetId(entity);
        var index = Data.FindIndex(x => GetId(x) == id);
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        Data[index] = entity;
        return Task.FromResult(true);
    }

    private static int GetId(T item)
    {
        return item switch
        {
            Tour tour => tour.Id,
            BookingRequest booking => booking.Id,
            _ => throw new InvalidOperationException("Entity does not contain Id")
        };
    }

    private static void SetId(T item, int id)
    {
        switch (item)
        {
            case Tour tour:
                tour.Id = id;
                break;
            case BookingRequest booking:
                booking.Id = id;
                break;
            default:
                throw new InvalidOperationException("Entity does not contain Id");
        }
    }
}
