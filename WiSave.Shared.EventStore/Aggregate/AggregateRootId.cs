namespace WiSave.Shared.EventStore.Aggregate;

public record AggregateRootId(Guid Id)
{
    public static implicit operator Guid(AggregateRootId aggregateRootId) => aggregateRootId.Id;
    public static implicit operator string(AggregateRootId aggregateRootId) => aggregateRootId.ToString();
}