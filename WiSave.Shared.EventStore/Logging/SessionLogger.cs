using Marten;
using Marten.Services;
using Microsoft.Extensions.Logging;
using Npgsql;
using WiSave.Shared.EventStore.Session;

namespace WiSave.Shared.EventStore.Logging;

public class CorrelatedMartenLogger(ILogger<IDocumentSession> logger, ISession session) : IMartenSessionLogger
{
    private readonly ILogger<IDocumentSession> _logger = logger;
    private readonly ISession _session = session;

    public void LogSuccess(NpgsqlCommand command)
    {
        // Do some kind of logging using the correlation id of the ISession
    }

    public void LogFailure(NpgsqlCommand command, Exception ex)
    {
        // Do some kind of logging using the correlation id of the ISession
    }

    public void LogSuccess(NpgsqlBatch batch)
    {
        // Do some kind of logging using the correlation id of the ISession
    }

    public void LogFailure(NpgsqlBatch batch, Exception ex)
    {
        // Do some kind of logging using the correlation id of the ISession
    }

    public void RecordSavedChanges(IDocumentSession session, IChangeSet commit)
    {
        // Do some kind of logging using the correlation id of the ISession
    }

    public void OnBeforeExecute(NpgsqlCommand command)
    {
    }

    public void LogFailure(Exception ex, string message)
    {
    }

    public void OnBeforeExecute(NpgsqlBatch batch)
    {
    }
}