namespace Common;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a base payload for vector db.
/// </summary>
[Serializable]
public class QdrantDocumentVectorPayload : VectorDbPayload
{
    [Required]
    [JsonInclude]
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("page")]
    public string? Page { get; set; }
}
