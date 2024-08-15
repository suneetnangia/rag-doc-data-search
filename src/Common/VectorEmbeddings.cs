namespace Common;

using System.Text.Json.Serialization;

public class VectorEmbeddings {
    [JsonInclude]
    public required float[] Embedding;
}
