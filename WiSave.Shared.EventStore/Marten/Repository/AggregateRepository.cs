using JasperFx.Events;
using Marten;
using WiSave.Shared.EventStore.Aggregate;

namespace WiSave.Shared.EventStore.Marten.Repository;

public class AggregateRepository<TAggregate>(IDocumentSession documentSession) : IAggregateRepository<TAggregate> 
    where TAggregate : class, IAggregate
{
    public Task<TAggregate?> Find(Guid id, CancellationToken ct = default) =>
        documentSession.Events.AggregateStreamAsync<TAggregate>(id, token: ct);

    public Task<TAggregate?> GetById(Guid id, CancellationToken ct = default) =>
        Find(id, ct);

    public async Task<long> Add(Guid id, TAggregate aggregate, CancellationToken ct = default)
    {
        var events = aggregate.DequeueUncommittedEvents();
        documentSession.Events.StartStream<AggregateRootId>(id, events);
        await documentSession.SaveChangesAsync(ct);
        return events.Length;
    }

    public async Task<long> Update(Guid id, TAggregate aggregate, long? expectedVersion = null, CancellationToken ct = default)
    {
        var events = aggregate.DequeueUncommittedEvents();
        if (events.Length == 0)
            return aggregate.Version;

        var nextVersion = (expectedVersion ?? aggregate.Version) + events.Length;
        documentSession.Events.Append(id, nextVersion, events);
        await documentSession.SaveChangesAsync(ct);
        return nextVersion;
    }

    public Task<long> Delete(Guid id, TAggregate aggregate, long? expectedVersion = null, CancellationToken ct = default) =>
        Update(id, aggregate, expectedVersion, ct);

    public async Task<IEnumerable<TAggregate>> FindMany(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var results = new List<TAggregate>();
        foreach (var id in ids)
        {
            var aggregate = await Find(id, ct);
            if (aggregate != null)
                results.Add(aggregate);
        }

        return results;
    }

    public async Task<long> AddMany(IEnumerable<(Guid Id, TAggregate Aggregate)> aggregates, CancellationToken ct = default)
    {
        long totalEvents = 0;
        foreach (var (id, aggregate) in aggregates)
        {
            var events = aggregate.DequeueUncommittedEvents();
            documentSession.Events.StartStream<AggregateRootId>(id, events);
            totalEvents += events.Length;
        }

        await documentSession.SaveChangesAsync(ct);
        return totalEvents;
    }

    public async Task<long> UpdateMany(IEnumerable<(Guid Id, TAggregate Aggregate, long? ExpectedVersion)> aggregates, CancellationToken ct = default)
    {
        long totalEvents = 0;
        foreach (var (id, aggregate, expectedVersion) in aggregates)
        {
            var events = aggregate.DequeueUncommittedEvents();
            if (events.Any())
            {
                var nextVersion = (expectedVersion ?? aggregate.Version) + events.Length;
                documentSession.Events.Append(id, nextVersion, events);
                totalEvents += events.Length;
            }
        }

        await documentSession.SaveChangesAsync(ct);
        return totalEvents;
    }
    
    public async Task<IEvent[]> GetEvents(Guid id, CancellationToken ct = default)
    {
        var events = await documentSession.Events.FetchStreamAsync(id, token: ct);
        return events.ToArray();
    }

    public async Task<IEvent[]> GetEvents(Guid id, long fromVersion, CancellationToken ct = default)
    {
        var events = await documentSession.Events.FetchStreamAsync(id, fromVersion, token: ct);
        return events.ToArray();
    }

    public async Task<IEvent[]> GetEvents(Guid id, long fromVersion, long toVersion, CancellationToken ct = default)
    {
        var events = await documentSession.Events.FetchStreamAsync(id, fromVersion, token: ct);
        return events.Where(e => e.Version <= toVersion).ToArray();
    }

    public async Task<IEvent[]> GetEvents(Guid id, DateTime fromTimestamp, CancellationToken ct = default)
    {
        var events = await documentSession.Events.FetchStreamAsync(id, timestamp: fromTimestamp, token: ct);
        return events.ToArray();
    }

    public async Task<IEvent[]> GetEvents(Guid id, DateTime fromTimestamp, DateTime toTimestamp, CancellationToken ct = default)
    {
        var events = await documentSession.Events.FetchStreamAsync(id, timestamp: fromTimestamp, token: ct);
        return events.Where(e => e.Timestamp <= toTimestamp).ToArray();
    }

    public async Task<TAggregate?> GetAtVersion(Guid id, long version, CancellationToken ct = default)
    {
        return await documentSession.Events.AggregateStreamAsync<TAggregate>(id, version, token: ct);
    }
    
    public async Task<long> AppendEvents(Guid id, IEnumerable<object> events, long? expectedVersion = null, CancellationToken ct = default)
    {
        var eventArray = events.ToArray();
        if (eventArray.Length == 0) return expectedVersion ?? 0;

        var nextVersion = (expectedVersion ?? 0) + eventArray.Length;
        documentSession.Events.Append(id, expectedVersion ?? 0, eventArray);
        await documentSession.SaveChangesAsync(ct);
        return nextVersion;
    }

    public async Task<long> AppendEventsWithMetadata(Guid id, IEnumerable<(object Event, Dictionary<string, object> Metadata)> eventsWithMetadata, long? expectedVersion = null,
        CancellationToken ct = default)
    {
        var eventArray = eventsWithMetadata.ToArray();
        if (!eventArray.Any()) return expectedVersion ?? 0;

        var nextVersion = (expectedVersion ?? 0) + eventArray.Length;

        foreach (var (eventData, metadata) in eventArray)
        {
            documentSession.Events.Append(id, eventData, metadata);
        }

        await documentSession.SaveChangesAsync(ct);
        return nextVersion;
    }
    
    public async Task<bool> StreamExists(Guid id, CancellationToken ct = default)
    {
        var state = await documentSession.Events.FetchStreamStateAsync(id, ct);
        return state != null;
    }

    public async Task<long> GetCurrentVersion(Guid id, CancellationToken ct = default)
    {
        var state = await documentSession.Events.FetchStreamStateAsync(id, ct);
        return state?.Version ?? 0;
    }

    public async Task<DateTimeOffset?> GetLastModified(Guid id, CancellationToken ct = default)
    {
        var state = await documentSession.Events.FetchStreamStateAsync(id, ct);
        return state?.LastTimestamp;
    }
}