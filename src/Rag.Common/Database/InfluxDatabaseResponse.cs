namespace Rag.Common.Database;

using System.Text.Json.Serialization;
using InfluxDB.Client.Core.Flux.Domain;

/// <summary>
/// TODO: Add more properties to this class as needed.
/// </summary>
[Serializable]
public class InfluxDatabaseResponse
{
    [JsonInclude]
    public required FluxTable[] Raw { get; set; }
}
