namespace Common;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a response from vector db.
/// </summary>
[Serializable]
public class VectorResponse {
    [JsonInclude]
    public required string Id;

    [JsonInclude]
    public required float Score;

    [JsonInclude]
    public required string Text;    
}
