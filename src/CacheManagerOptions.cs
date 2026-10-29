namespace XperienceCommunity.CacheManager;

/// <summary>
/// Options to customize the Cache Manager module.
/// </summary>
public class CacheManagerOptions
{
    /// <summary>
    /// The maximum number of custom buttons that can be registered in <see cref="ClearCacheButtons"/>.
    /// </summary>
    public const int MAX_CLEAR_CACHE_BUTTONS = 10;

    /// <summary>
    /// Custom buttons displayed next to the "Clear Cache" button, each clearing only the cache items whose key contains a given value.
    /// </summary>
    public IList<ClearCacheButton> ClearCacheButtons { get; } = [];
}

/// <summary>
/// A custom button that clears all cache items whose key contains <see cref="CacheKeyContains"/>.
/// </summary>
public class ClearCacheButton
{
    /// <summary>
    /// The text displayed on the button.
    /// </summary>
    public required string ButtonText { get; set; }

    /// <summary>
    /// The value searched for in the cache keys (case-insensitive), e.g. "contentitem|".
    /// </summary>
    public required string CacheKeyContains { get; set; }
}
