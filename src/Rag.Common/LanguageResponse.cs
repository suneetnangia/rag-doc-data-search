namespace Rag.Common;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a response from the language model returned from Ollama.
/// </summary>
[Serializable]
public class LanguageResponse
{
    [JsonInclude]
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonInclude]
    [JsonPropertyName("created_at")]
    public required string CreatedAt { get; set; }

    [JsonInclude]
    [JsonPropertyName("response")]
    public required string Response { get; set; }

    [JsonInclude]
    [JsonPropertyName("done")]
    public required bool Done { get; set; }

    [JsonInclude]
    [JsonPropertyName("done_reason")]
    public required string DoneReason { get; set; }

    [JsonInclude]
    [JsonPropertyName("context")]
    public int[]? Context { get; set; }
}
