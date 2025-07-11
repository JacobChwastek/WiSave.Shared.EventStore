namespace WiSave.Shared.EventStore.Aggregate;

public abstract record Event<TId>(TId AggregateId) where TId : AggregateRootId;