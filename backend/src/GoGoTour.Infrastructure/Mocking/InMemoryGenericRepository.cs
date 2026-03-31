using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoGoTour.Application.Abstractions;
using GoGoTour.Domain.Entities;

namespace GoGoTour.Infrastructure.Mocking
{
    public class InMemoryGenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly InMemoryStore _store;

        public InMemoryGenericRepository(InMemoryStore store)
        {
            _store = store;
        }

        private List<T> Data
        {
            get
            {
                if (typeof(T) == typeof(Tour))
                {
                    return (List<T>)(object)_store.Tours;
                }

                if (typeof(T) == typeof(BookingRequest))
                {
                    return (List<T>)(object)_store.BookingRequests;
                }

                throw new InvalidOperationException("Type " + typeof(T).Name + " is not supported by in-memory store");
            }
        }

        public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult((IReadOnlyList<T>)Data.ToList());
        }

        public Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entity = Data.FirstOrDefault(x => GetId(x) == id);
            return Task.FromResult(entity);
        }

        public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            var nextId = Data.Count == 0 ? 1 : Data.Max(GetId) + 1;
            SetId(entity, nextId);
            Data.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default(CancellationToken))
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
            var tour = item as Tour;
            if (tour != null)
            {
                return tour.Id;
            }

            var booking = item as BookingRequest;
            if (booking != null)
            {
                return booking.Id;
            }

            throw new InvalidOperationException("Entity does not contain Id");
        }

        private static void SetId(T item, int id)
        {
            var tour = item as Tour;
            if (tour != null)
            {
                tour.Id = id;
                return;
            }

            var booking = item as BookingRequest;
            if (booking != null)
            {
                booking.Id = id;
                return;
            }

            throw new InvalidOperationException("Entity does not contain Id");
        }
    }
}
