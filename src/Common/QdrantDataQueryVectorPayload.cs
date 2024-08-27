namespace Common;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a payload for a data query.
/// </summary>
[Serializable]
public class QdrantQueryDataVectorPayload : VectorDbPayload
{
    [Required]
    [JsonInclude]
    [JsonPropertyName("query")]
    public required string Query { get; set; }

}
