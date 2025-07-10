using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WiSave.Shared.EventStore.Aggregate;
using WiSave.Shared.EventStore.Marten.OpenTelemetry;
using WiSave.Shared.OpenTelemetry.OpenTelemetry;

namespace WiSave.Shared.EventStore.Marten.Repository;

public static class Extensions
{
    public static IServiceCollection AddMartenRepository<T>(this IServiceCollection services) where T : class, IAggregate
    {
        services.AddScoped<AggregateRepository<T>>();
        services.AddScoped<IAggregateRepository<T>>(sp =>
        {
            var inner = sp.GetRequiredService<AggregateRepository<T>>();
            var session = sp.GetRequiredService<IDocumentSession>();
            var scope = sp.GetRequiredService<IActivityScope>();
            var logger = sp.GetRequiredService<ILogger<AggregateRepositoryWithTracingDecorator<T>>>();

            return new AggregateRepositoryWithTracingDecorator<T>(inner, session, scope, logger);
        });

        return services;
    }
}