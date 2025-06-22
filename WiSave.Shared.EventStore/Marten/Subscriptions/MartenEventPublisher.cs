using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Subscriptions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WiSave.Shared.OpenTelemetry.OpenTelemetry;

namespace WiSave.Shared.EventStore.Marten.Subscriptions;

public class MartenEventPublisher<TBus>(
    IServiceProvider serviceProvider,
    IActivityScope activityScope,
    ILogger<MartenEventPublisher<TBus>> logger
) : SubscriptionBase where TBus : IBus
{
    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange eventRange,
        ISubscriptionController subscriptionController,
        IDocumentOperations operations,
        CancellationToken token
    )
    {
        var lastProcessed = eventRange.SequenceFloor;
        try
        {
            foreach (var @event in eventRange.Events)
            {
                var parentContext =
                    TelemetryPropagator.Extract(@event.Headers, ExtractTraceContextFromEventMetadata);

                await activityScope.Run($"{nameof(MartenEventPublisher<TBus>)}/{nameof(ProcessEventsAsync)}",
                    async (_, ct) =>
                    {
                        using var scope = serviceProvider.CreateScope();
                        var eventBus = scope.ServiceProvider.GetRequiredService<TBus>();

                        await eventBus.Publish(@event.Data, ct)
                            .ConfigureAwait(false);
                    },
                    new StartActivityOptions
                    {
                        Tags = { { TelemetryTags.EventHandling.Event, @event.Data.GetType() } },
                        Parent = parentContext.ActivityContext
                    },
                    token
                ).ConfigureAwait(false);
            }

            return NullChangeListener.Instance;
        }
        catch (Exception exc)
        {
            logger.LogError("Error while processing Marten Subscription: {ExceptionMessage}", exc.Message);
            await subscriptionController.ReportCriticalFailureAsync(exc, lastProcessed).ConfigureAwait(false);
            throw;
        }
    }

    private IEnumerable<string> ExtractTraceContextFromEventMetadata(Dictionary<string, object>? headers, string key)
    {
        try
        {
            if (headers!.TryGetValue(key, out var value) != true)
                return [];

            var stringValue = value.ToString();

            return stringValue != null
                ? [stringValue]
                : [];
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to extract trace context: {ex}", ex);
            return [];
        }
    }
}