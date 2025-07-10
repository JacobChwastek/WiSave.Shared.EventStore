using JasperFx.Events;

namespace WiSave.Shared.EventStore.Marten.Repository;

public static class AggregateRepositoryExtensions
{
    public static async Task<T> Get<T>(this IAggregateRepository<T> repository, Guid id, CancellationToken cancellationToken = default) where T : class
    {
        var entity = await repository.Find(id, cancellationToken).ConfigureAwait(false);
        
        return entity ?? throw new InvalidOperationException($"Aggregate with ID {id} not found");
    }

    public static async Task<long> GetAndUpdate<T>(
        this IAggregateRepository<T> repository,
        Guid id,
        Action<T> action,
        long? expectedVersion = null,
        CancellationToken ct = default
    ) where T : class
    {
        var entity = await repository.GetById(id, ct).ConfigureAwait(false);

        action(entity);

        return await repository.Update(id, entity, expectedVersion, ct).ConfigureAwait(false);
    }
    
    public static async Task<bool> Exists<T>(this IAggregateRepository<T> repository, Guid id, CancellationToken ct = default) 
        where T : class
    {
        return await repository.StreamExists(id, ct);
    }

    public static async Task<T> GetByIdOrThrow<T>(this IAggregateRepository<T> repository, Guid id, CancellationToken ct = default) 
        where T : class
    {
        var result = await repository.GetById(id, ct);
        return result ?? throw new InvalidOperationException($"Aggregate with ID {id} not found");
    }

    public static async Task<T?> GetAtVersionSafe<T>(this IAggregateRepository<T> repository, Guid id, long version, CancellationToken ct = default) 
        where T : class
    {
        try
        {
            return await repository.GetAtVersion(id, version, ct);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<bool> HasEvents<T>(this IAggregateRepository<T> repository, Guid id, CancellationToken ct = default) 
        where T : class
    {
        var events = await repository.GetEvents(id, ct);
        return events.Any();
    }

    public static async Task<int> GetEventCount<T>(this IAggregateRepository<T> repository, Guid id, CancellationToken ct = default) 
        where T : class
    {
        var events = await repository.GetEvents(id, ct);
        return events.Length;
    }

    public static async Task<IEvent[]> GetEventsByType<T>(this IAggregateRepository<T> repository, Guid id, Type eventType, CancellationToken ct = default) 
        where T : class
    {
        var events = await repository.GetEvents(id, ct);
        return events.Where(e => e.Data.GetType() == eventType).ToArray();
    }

    public static async Task<IEvent[]> GetRecentEvents<T>(this IAggregateRepository<T> repository, Guid id, int count, CancellationToken ct = default) 
        where T : class
    {
        var events = await repository.GetEvents(id, ct);
        return events.TakeLast(count).ToArray();
    }
}