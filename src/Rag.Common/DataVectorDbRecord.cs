namespace Rag.Common;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[Serializable]
public class DataVectorDbRecord : BaseVectorDbRecord
{
    [Required]
    [JsonInclude]
    [JsonPropertyName("query")]
    public required string Query { get; set; }
}
