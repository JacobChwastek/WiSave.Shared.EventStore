using JasperFx.Events;
using Marten;
using Microsoft.Extensions.Logging;
using WiSave.Shared.EventStore.Aggregate;
using WiSave.Shared.EventStore.Marten.Repository;
using WiSave.Shared.OpenTelemetry.OpenTelemetry;

namespace WiSave.Shared.EventStore.Marten.OpenTelemetry;

public class AggregateRepositoryWithTracingDecorator<T>(
    IAggregateRepository<T> inner,
    IDocumentSession documentSession,
    IActivityScope activityScope,
    ILogger<AggregateRepositoryWithTracingDecorator<T>> logger)
    : IAggregateRepository<T>
    where T : class, IAggregate
{
    public Task<T?> Find(Guid id, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(Find)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.Find(id, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id }
                }
            },
            cancellationToken
        );

    public Task<T?> GetById(Guid id, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(GetById)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.GetById(id, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id }
                }
            },
            cancellationToken
        );

    public Task<long> Add(Guid id, T aggregate, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(Add)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.Add(id, aggregate, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { TelemetryTags.Logic.Entities.EntityVersion, aggregate.Version }
                }
            },
            cancellationToken
        );

    public Task<long> Update(Guid id, T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(Update)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.Update(id, aggregate, expectedVersion, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { TelemetryTags.Logic.Entities.EntityVersion, aggregate.Version },
                    { "marten.expected_version", expectedVersion?.ToString() ?? "null" }
                }
            },
            cancellationToken
        );

    public Task<long> Delete(Guid id, T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(Delete)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.Delete(id, aggregate, expectedVersion, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { TelemetryTags.Logic.Entities.EntityVersion, aggregate.Version },
                    { "marten.expected_version", expectedVersion?.ToString() ?? "null" }
                }
            },
            cancellationToken
        );
    
    public Task<IEnumerable<T>> FindMany(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(FindMany)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.FindMany(ids, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { "marten.bulk_operation.count", ids.Count() },
                    { "marten.bulk_operation.type", "find_many" }
                }
            },
            cancellationToken
        );

    public Task<long> AddMany(IEnumerable<(Guid Id, T Aggregate)> aggregates, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(AddMany)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.AddMany(aggregates, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { "marten.bulk_operation.count", aggregates.Count() },
                    { "marten.bulk_operation.type", "add_many" }
                }
            },
            cancellationToken
        );

    public Task<long> UpdateMany(IEnumerable<(Guid Id, T Aggregate, long? ExpectedVersion)> aggregates, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(UpdateMany)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.UpdateMany(aggregates, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { "marten.bulk_operation.count", aggregates.Count() },
                    { "marten.bulk_operation.type", "update_many" }
                }
            },
            cancellationToken
        );

    // Event operations
    public Task<IEvent[]> GetEvents(Guid id, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(GetEvents)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.GetEvents(id, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { "marten.event_operation.type", "get_all_events" }
                }
            },
            cancellationToken
        );

    public Task<IEvent[]> GetEvents(Guid id, long fromVersion, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(GetEvents)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.GetEvents(id, fromVersion, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { "marten.event_operation.type", "get_events_from_version" },
                    { "marten.event_operation.from_version", fromVersion }
                }
            },
            cancellationToken
        );

    public Task<IEvent[]> GetEvents(Guid id, long fromVersion, long toVersion, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(GetEvents)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.GetEvents(id, fromVersion, toVersion, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { "marten.event_operation.type", "get_events_version_range" },
                    { "marten.event_operation.from_version", fromVersion },
                    { "marten.event_operation.to_version", toVersion }
                }
            },
            cancellationToken
        );

    public Task<IEvent[]> GetEvents(Guid id, DateTime fromTimestamp, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(GetEvents)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.GetEvents(id, fromTimestamp, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { "marten.event_operation.type", "get_events_from_timestamp" },
                    { "marten.event_operation.from_timestamp", fromTimestamp.ToString("O") }
                }
            },
            cancellationToken
        );

    public Task<IEvent[]> GetEvents(Guid id, DateTime fromTimestamp, DateTime toTimestamp, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(GetEvents)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.GetEvents(id, fromTimestamp, toTimestamp, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { "marten.event_operation.type", "get_events_timestamp_range" },
                    { "marten.event_operation.from_timestamp", fromTimestamp.ToString("O") },
                    { "marten.event_operation.to_timestamp", toTimestamp.ToString("O") }
                }
            },
            cancellationToken
        );
    
    public Task<T?> GetAtVersion(Guid id, long version, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(GetAtVersion)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.GetAtVersion(id, version, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { TelemetryTags.Logic.Entities.EntityVersion, version },
                    { "marten.operation.type", "get_at_version" }
                }
            },
            cancellationToken
        );
    
    public Task<long> AppendEvents(Guid id, IEnumerable<object> events, long? expectedVersion = null, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(AppendEvents)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.AppendEvents(id, events, expectedVersion, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { "marten.event_operation.type", "append_events" },
                    { "marten.event_operation.count", events.Count() },
                    { "marten.expected_version", expectedVersion?.ToString() ?? "null" }
                }
            },
            cancellationToken
        );

    public Task<long> AppendEventsWithMetadata(Guid id, IEnumerable<(object Event, Dictionary<string, object> Metadata)> eventsWithMetadata, long? expectedVersion = null, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(AppendEventsWithMetadata)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.AppendEventsWithMetadata(id, eventsWithMetadata, expectedVersion, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { "marten.event_operation.type", "append_events_with_metadata" },
                    { "marten.event_operation.count", eventsWithMetadata.Count() },
                    { "marten.expected_version", expectedVersion?.ToString() ?? "null" }
                }
            },
            cancellationToken
        );
    
    public Task<bool> StreamExists(Guid id, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(StreamExists)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.StreamExists(id, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { "marten.operation.type", "stream_exists" }
                }
            },
            cancellationToken
        );

    public Task<long> GetCurrentVersion(Guid id, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(GetCurrentVersion)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.GetCurrentVersion(id, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { "marten.operation.type", "get_current_version" }
                }
            },
            cancellationToken
        );

    public Task<DateTimeOffset?> GetLastModified(Guid id, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(GetLastModified)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);
                return inner.GetLastModified(id, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { "marten.operation.type", "get_last_modified" }
                }
            },
            cancellationToken
        );
}