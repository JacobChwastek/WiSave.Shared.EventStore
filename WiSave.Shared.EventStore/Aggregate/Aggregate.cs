using System.Text.Json.Serialization;
using Marten.Schema;

namespace WiSave.Shared.EventStore.Aggregate;

public abstract class Aggregate<TKey> : IAggregate where TKey : AggregateRootId
{
    public TKey Id { get; set; } = null!;

    [Identity]
    public Guid AggregateId
    {
        get => Id.Id;
        set { }
    }

    public int Version { get; protected set; }

    [JsonIgnore] private readonly Queue<object> _uncommittedEvents = new();

    public object[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = _uncommittedEvents.ToArray();

        _uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    protected void Enqueue(object @event)
    {
        _uncommittedEvents.Enqueue(@event);
    }
}