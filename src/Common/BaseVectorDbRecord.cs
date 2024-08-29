namespace Common;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[Serializable]
public class BaseVectorDbRecord
{
    [Required]  
    [JsonInclude]
    [JsonPropertyName("document")]
    public required string Document { get; set; } = string.Empty;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tags")]
    public string? Tags { get; set; }

}
