namespace Common.Caching
{
    /// <summary>
    /// Unified cache settings for all microservices.
    /// Configure in appsettings.json under "CacheSettings" section.
    /// </summary>
    public class CacheSettings
    {
        /// <summary>
        /// Cache expiration time in minutes. Default is 5 minutes.
        /// </summary>
        public int CacheExpirationMinutes { get; set; } = 5;

        /// <summary>
        /// When true, caching is disabled and all cache operations become no-ops.
        /// Useful for development and debugging.
        /// </summary>
        public bool Disabled { get; set; } = false;

        /// <summary>
        /// Sliding expiration in minutes. If set, cache items expire after this time of inactivity.
        /// Set to 0 to disable sliding expiration.
        /// </summary>
        public int SlidingExpirationMinutes { get; set; } = 0;

        /// <summary>
        /// Maximum number of items to cache. Set to 0 for unlimited.
        /// </summary>
        public int MaxCacheItems { get; set; } = 0;
    }
}
