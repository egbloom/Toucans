namespace ToucansApi.Functions.Configurations

{
public class Settings
{
    public required string ConnectionString { get; set; }
    public int DefaultPageSize { get; set; }
    public int MaxPageSize { get; set; }
    public bool EnableSwagger { get; set; }
}
}