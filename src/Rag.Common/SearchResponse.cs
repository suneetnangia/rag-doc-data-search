namespace Rag.Common;

using System.Text.Json.Serialization;
using Rag.Common.LanguageModel;

/// <summary>
/// Represents a combined response from vector db and S/LLMs optionally.
/// </summary>
[Serializable]
public class SearchResponse
{
    [JsonInclude]
    public required VectorResponse VectorResponse { get; set; }

    [JsonInclude]
    public LanguageResponse? LanguageResponse { get; set; }
}
