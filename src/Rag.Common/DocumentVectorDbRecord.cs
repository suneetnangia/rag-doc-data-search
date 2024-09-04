namespace Rag.Common;

using System.Text.Json.Serialization;

[Serializable]
public class DocumentVectorDbRecord : BaseVectorDbRecord
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("filename")]
    public string? FileName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("page")]
    public string? Page { get; set; }
}
