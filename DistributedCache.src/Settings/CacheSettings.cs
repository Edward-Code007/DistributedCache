namespace DistributedCache.Settings;

public class CacheSettings
{
    public const string SectionName = "Cache";
    public int AbsoluteExpirationMinutes { get; set; } = 5;
    public int SlidingExpirationMinutes { get; set; } = 2;
    public string ProductKeyPrefix { get; set; } = "product";
}
