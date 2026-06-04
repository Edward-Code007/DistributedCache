public class RedisSettings
{
    public const string SectionName = "Redis";
    public string Hostname{get;set;} = "localhost";
    public string Port{get;set;} = "6379";
    public string InstanceName{get;set;} = "DistributedCache";
    public string ConnectionString => $"{Hostname}:{Port}";
}