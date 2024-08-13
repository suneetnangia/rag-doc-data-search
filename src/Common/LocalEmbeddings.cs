namespace Common;

using System.Text.Json.Serialization;

public class LocalEmbeddings {
    [JsonInclude]
    public required float[] Embedding;
}
