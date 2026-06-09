namespace DistributedCache.Settings;

public class DatabaseSettings
{
    public const string SectionName = "Database";
    public string Name { get; set; } = "ProductsDb";
    public string User {get;set;} = "user";
    public string Password{get;set;} = "user";
    public string Hostname{get;set;} = "localhost";
    public string DbName {get;set;} = "db";
    public string Port{get;set;} = "5432";
    public string StringConnection => $"Host={Hostname};Port={Port};Database={DbName};Username={User};Password={Password}";
}
