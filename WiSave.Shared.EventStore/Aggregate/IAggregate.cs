namespace WiSave.Shared.EventStore.Aggregate;

public interface IAggregate
{
    int Version { get; }

    object[] DequeueUncommittedEvents();
}