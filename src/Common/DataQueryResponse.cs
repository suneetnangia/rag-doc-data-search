namespace Common;

using System.Text.Json.Serialization;

[Serializable]
public class DataQueryResponse {
    [JsonInclude]
    public required VectorResponse VectorResponse;
    [JsonInclude]
    public required InfluxDatabaseResponse DatabaseQueryResponse;
    [JsonInclude]
    public LanguageResponse? LanguageResponse;
}
