namespace Rag.Common.LanguageModel;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a response from vector db.
/// </summary>
[Serializable]
public class VectorResponse
{
    [JsonInclude]
    public required string Id { get; set; }

    [JsonInclude]
    public required float Score { get; set; }

    [JsonInclude]
    public required string Text { get; set; }
}
