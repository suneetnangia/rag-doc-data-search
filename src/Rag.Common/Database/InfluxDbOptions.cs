namespace Rag.Common.Database;

/// <summary>
/// Options for Influx database repository.
/// </summary>
public class InfluxDbOptions
{
    public const string InfluxDb = "InfluxDb";

    public required string Url { get; set; }

    public required string Token { get; set; }
}
