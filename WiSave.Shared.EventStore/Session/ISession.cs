namespace WiSave.Shared.EventStore.Session;

public interface ISession
{
    Guid CorrelationId { get; set; }
}