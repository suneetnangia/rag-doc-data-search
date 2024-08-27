namespace Common;
using System.ComponentModel.DataAnnotations;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a base payload for vector db.
/// </summary>
[Serializable]
public class VectorDbPayload
{
    [Required]
    [JsonInclude]
    [JsonPropertyName("document")]
    public required string Document { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tags")]
    public string? Tags { get; set; }
}
