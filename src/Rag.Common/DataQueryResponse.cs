namespace Rag.Common;

using System.Text.Json.Serialization;
using Rag.Common.Database;
using Rag.Common.LanguageModel;

[Serializable]
public class DataQueryResponse
{
    [JsonInclude]
    public required VectorResponse VectorResponse { get; set; }

    [JsonInclude]
    public required InfluxDatabaseResponse DatabaseQueryResponse { get; set; }

    [JsonInclude]
    public required LanguageResponse? LanguageResponse { get; set; }
}
