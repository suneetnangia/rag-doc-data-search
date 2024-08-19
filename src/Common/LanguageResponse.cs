namespace Common;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a response from the language model returned from Ollama.
/// </summary>
[Serializable]
public class LanguageResponse {
    [JsonInclude]
    [JsonPropertyName("model")]
    public required string Model;

    [JsonInclude()]
    [JsonPropertyName("created_at")]
    public required string CreatedAt;

    [JsonInclude]
    [JsonPropertyName("response")]
    public required string Response;

    [JsonInclude]
    [JsonPropertyName("done")]
    public required bool Done;

    [JsonInclude]
    [JsonPropertyName("done_reason")]
    public required string DoneReason;

    [JsonInclude]
    [JsonPropertyName("context")]
    public int[]? Context;
}
