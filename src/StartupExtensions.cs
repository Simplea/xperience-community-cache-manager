using Microsoft.Extensions.DependencyInjection;

using XperienceCommunity.CacheManager.Services;


namespace XperienceCommunity.CacheManager;

/// <summary>
/// Contains methods to initialize the module during application startup.
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Registers services required by the module.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional delegate to customize the module, e.g. to add custom clear cache buttons.</param>
    public static IServiceCollection AddCacheManager(this IServiceCollection services, Action<CacheManagerOptions>? configure = null)
    {
        var options = new CacheManagerOptions();
        configure?.Invoke(options);
        ValidateOptions(options);

        services.AddSingleton(options);
        services.AddSingleton<ICacheManagerProvider, CacheManagerProvider>();

        return services;
    }

    private static void ValidateOptions(CacheManagerOptions options)
    {
        if (options.ClearCacheButtons.Count > CacheManagerOptions.MAX_CLEAR_CACHE_BUTTONS)
        {
            throw new InvalidOperationException(
                $"The Cache Manager supports up to {CacheManagerOptions.MAX_CLEAR_CACHE_BUTTONS} custom clear cache buttons, but {options.ClearCacheButtons.Count} were registered.");
        }

        foreach (var button in options.ClearCacheButtons)
        {
            if (string.IsNullOrWhiteSpace(button.ButtonText) || string.IsNullOrWhiteSpace(button.CacheKeyContains))
            {
                throw new InvalidOperationException(
                    $"Custom clear cache buttons require both {nameof(ClearCacheButton.ButtonText)} and {nameof(ClearCacheButton.CacheKeyContains)} to be set.");
            }
        }
    }
}
