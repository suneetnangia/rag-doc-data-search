namespace Common;

/// <summary>
/// Options for the Ollama.
/// </summary>
public class OllamaOptions
{
    public const string VectorDb = "Ollama";    
    public required string OllamaApiBaseUrl { get; set; }
    public required string OllamaApiPullRelativeUrl { get; set; }
    public required string OllamaApiResponseRelativeUrl { get; set; }
    public required string OllamaApiEmbeddingsRelativeUrl { get; set; }
    public required string EmbeddingsLanguageModelName { get; set; }
    public required string ResponseLanguageModelName { get; set; }
}
