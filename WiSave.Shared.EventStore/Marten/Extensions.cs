using JasperFx;
using Marten;
using Marten.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using WiSave.Shared.EventStore.Configuration;
using WiSave.Shared.EventStore.Marten.Subscriptions;

namespace WiSave.Shared.EventStore.Marten;

public static class MartenConfigExtensions
{
    private const string DefaultConfigKey = "EventStore";

    public static MartenServiceCollectionExtensions.MartenConfigurationExpression AddMarten<TBus>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<StoreOptions>? configureOptions = null,
        string configKey = DefaultConfigKey,
        bool disableAsyncDaemon = false
    ) where TBus : IBus
    {
        return services.AddMarten<TBus>(
            configuration.GetRequiredConfig<MartenConfig>(configKey),
            configureOptions,
            disableAsyncDaemon
        );
    }

    public static MartenServiceCollectionExtensions.MartenConfigurationExpression AddMarten<TBus>(
        this IServiceCollection services,
        MartenConfig martenConfig,
        Action<StoreOptions>? configureOptions = null,
        bool disableAsyncDaemon = false
    ) where TBus : IBus
    {
        var config = services
            .AddMarten(sp =>
            {
                var dataSource = sp.GetService<NpgsqlDataSource>();
                if (dataSource == null)
                    return SetStoreOptions(martenConfig, configureOptions);

                martenConfig.ConnectionString = dataSource.ConnectionString;

                return SetStoreOptions(martenConfig, configureOptions);
            })
            .UseLightweightSessions()
            .AssertDatabaseMatchesConfigurationOnStartup()
            .ApplyAllDatabaseChangesOnStartup();

        if (!disableAsyncDaemon)
        {
            config.AddAsyncDaemon(martenConfig.DaemonMode)
                .AddSubscriptionWithServices<MartenEventPublisher<TBus>>(ServiceLifetime.Scoped);
        }

        return config;
    }

    private static StoreOptions SetStoreOptions(
        MartenConfig martenConfig,
        Action<StoreOptions>? configureOptions = null
    )
    {
        var options = new StoreOptions();
        options.Connection(martenConfig.ConnectionString);
        options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

        var schemaName = Environment.GetEnvironmentVariable("SchemaName");
        options.Events.DatabaseSchemaName = schemaName ?? martenConfig.WriteModelSchema;
        options.DatabaseSchemaName = schemaName ?? martenConfig.ReadModelSchema;

        options.UseSystemTextJsonForSerialization();

        options.Projections.Errors.SkipApplyErrors = false;
        options.Projections.Errors.SkipSerializationErrors = false;
        options.Projections.Errors.SkipUnknownEvents = false;

        if (martenConfig.UseMetadata)
        {
            options.Events.MetadataConfig.CausationIdEnabled = true;
            options.Events.MetadataConfig.CorrelationIdEnabled = true;
            options.Events.MetadataConfig.HeadersEnabled = true;
        }

        options.OpenTelemetry.TrackConnections = TrackLevel.Normal;
        options.OpenTelemetry.TrackEventCounters();

        configureOptions?.Invoke(options);

        return options;
    }
}