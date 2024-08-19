namespace Common;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a document, add any additional document attributes here.
/// </summary>
[Serializable]
public class VectorDocument {
    [JsonInclude]
    public required string Id;

    [JsonInclude]
    public required float Score;

    [JsonInclude]
    public required string Text;    
}
