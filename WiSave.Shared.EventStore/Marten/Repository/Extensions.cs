using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WiSave.Shared.EventStore.Aggregate;
using WiSave.Shared.EventStore.Marten.OpenTelemetry;
using WiSave.Shared.OpenTelemetry.OpenTelemetry;

namespace WiSave.Shared.EventStore.Marten.Repository;

public static class Extensions
{
    public static IServiceCollection AddMartenRepository<T>(this IServiceCollection services)
        where T : class, IAggregate
    {
        services.AddScoped<IMartenRepository<T>, MartenRepositoryWithTracingDecorator<T>>();
        services.Decorate<IMartenRepository<T>>((inner, sp) => new MartenRepositoryWithTracingDecorator<T>(
                inner,
                sp.GetRequiredService<IDocumentSession>(),
                sp.GetRequiredService<IActivityScope>(),
                sp.GetRequiredService<ILogger<MartenRepositoryWithTracingDecorator<T>>>()
            )
        );

        return services;
    }

    public static async Task<T> Get<T>(this IMartenRepository<T> repository, Guid id,
        CancellationToken cancellationToken = default) where T : class
    {
        var entity = await repository.Find(id, cancellationToken).ConfigureAwait(false);
        
        return entity ?? throw new Exception();
    }

    public static async Task<long> GetAndUpdate<T>(
        this IMartenRepository<T> repository,
        Guid id,
        Action<T> action,
        long? expectedVersion = null,
        CancellationToken ct = default
    ) where T : class
    {
        var entity = await repository.Get(id, ct).ConfigureAwait(false);

        action(entity);

        return await repository.Update(id, entity, expectedVersion, ct).ConfigureAwait(false);
    }
}