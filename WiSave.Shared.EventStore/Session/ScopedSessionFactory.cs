using Marten;
using Microsoft.Extensions.Logging;
using WiSave.Shared.EventStore.Logging;

namespace WiSave.Shared.EventStore.Session;

public class ScopedSessionFactory(IDocumentStore store, ILogger<IDocumentSession> logger, ISession session)
    : ISessionFactory
{
    public IQuerySession QuerySession()
    {
        return store.QuerySession();
    }

    public IDocumentSession OpenSession()
    {
        var newSession = store.LightweightSession();

        newSession.Logger = new CorrelatedMartenLogger(logger, session);

        return newSession;
    }
}