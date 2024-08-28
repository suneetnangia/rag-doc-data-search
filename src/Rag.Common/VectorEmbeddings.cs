namespace Rag.Common;

using System.Text.Json.Serialization;

public class VectorEmbeddings
{
    [JsonInclude]
    public required float[] Embedding { get; set; }
}
