namespace Common;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a combined response from vector db and S/LLMs optionally.
/// </summary>
[Serializable]
public class SearchResponse {
    [JsonInclude]
    public required VectorResponse VectorResponse;

    [JsonInclude]
    public LanguageResponse? LanguageResponse;
}
