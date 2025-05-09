using Microsoft.Extensions.Configuration;

namespace WiSave.Shared.EventStore.Configuration;

internal static class Extensions
{
    public static T GetRequiredConfig<T>(this IConfiguration configuration, string configurationKey) =>
        configuration.GetRequiredSection(configurationKey).Get<T>()
        ?? throw new InvalidOperationException(
            $"{typeof(T).Name} configuration wasn't found for '${configurationKey}' key");

    public static string GetRequiredConnectionString(this IConfiguration configuration, string configurationKey) =>
        configuration.GetConnectionString(configurationKey)
        ?? throw new InvalidOperationException(
            $"Configuration string with name '${configurationKey}' was not found");
}