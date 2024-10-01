namespace Rag.Common.LanguageModel;

/// <summary>
/// Options for Ollama.
/// </summary>
public class OllamaOptions
{
    public const string VectorDb = "Ollama";

    public required string OllamaApiBaseUrl { get; set; }

    // Ollama API timeout in seconds, depending on the model size and infrastructure it may take longer to generate embeddings/response.
    public required double HttpTimeoutInSeconds { get; set; } = 300;

    public required string OllamaApiPullRelativeUrl { get; set; }

    public required string OllamaApiResponseRelativeUrl { get; set; }

    public required string OllamaApiEmbeddingsRelativeUrl { get; set; }

    public required string EmbeddingsLanguageModelName { get; set; }

    public required string ResponseLanguageModelName { get; set; }
}
