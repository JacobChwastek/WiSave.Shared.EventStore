using JasperFx.Events;

namespace WiSave.Shared.EventStore.Marten.Repository;

public interface IAggregateRepository<T> where T : class
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken = default);
    Task<T?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<long> Add(Guid id, T aggregate, CancellationToken cancellationToken = default);
    Task<long> Update(Guid id, T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default);
    Task<long> Delete(Guid id, T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<T>> FindMany(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<long> AddMany(IEnumerable<(Guid Id, T Aggregate)> aggregates, CancellationToken cancellationToken = default);
    Task<long> UpdateMany(IEnumerable<(Guid Id, T Aggregate, long? ExpectedVersion)> aggregates, CancellationToken cancellationToken = default);
    
    Task<IEvent[]> GetEvents(Guid id, CancellationToken cancellationToken = default);
    Task<IEvent[]> GetEvents(Guid id, long fromVersion, CancellationToken cancellationToken = default);
    Task<IEvent[]> GetEvents(Guid id, long fromVersion, long toVersion, CancellationToken cancellationToken = default);
    Task<IEvent[]> GetEvents(Guid id, DateTime fromTimestamp, CancellationToken cancellationToken = default);
    Task<IEvent[]> GetEvents(Guid id, DateTime fromTimestamp, DateTime toTimestamp, CancellationToken cancellationToken = default);
    
    Task<T?> GetAtVersion(Guid id, long version, CancellationToken cancellationToken = default);
    
    Task<long> AppendEvents(Guid id, IEnumerable<object> events, long? expectedVersion = null, CancellationToken cancellationToken = default);
    Task<long> AppendEventsWithMetadata(Guid id, IEnumerable<(object Event, Dictionary<string, object> Metadata)> eventsWithMetadata, long? expectedVersion = null, CancellationToken cancellationToken = default);
    
    Task<bool> StreamExists(Guid id, CancellationToken cancellationToken = default);
    Task<long> GetCurrentVersion(Guid id, CancellationToken cancellationToken = default);
    Task<DateTimeOffset?> GetLastModified(Guid id, CancellationToken cancellationToken = default);
}
