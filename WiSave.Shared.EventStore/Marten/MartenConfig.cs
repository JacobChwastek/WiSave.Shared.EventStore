using Marten.Events.Daemon.Resiliency;

namespace WiSave.Shared.EventStore.Marten;

public class MartenConfig
{
    private const string DefaultSchema = "public";

    public string ConnectionString { get; set; } = null!;

    public string WriteModelSchema { get; set; } = DefaultSchema;
    public string ReadModelSchema { get; set; } = DefaultSchema;

    public bool ShouldRecreateDatabase { get; set; } = false;

    public DaemonMode DaemonMode { get; set; } = DaemonMode.Solo;

    public bool UseMetadata = true;
}