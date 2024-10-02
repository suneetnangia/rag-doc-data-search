namespace Rag.Common.LanguageModel;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rag.Common.Responses;

/// <summary>
/// Represents a model that generates embeddings for vector db.
/// </summary>
public class EmbeddingsModel : Model<VectorEmbeddings>
{
    private readonly ILogger _logger;
    private readonly HttpClient _http_client;
    private readonly Uri _ollama_embeddings_relative_url;

    public EmbeddingsModel(ILogger logger, HttpClient httpClient, IOptions<OllamaOptions> ollamaOptions)
    : base(
        logger,
        httpClient,
#pragma warning disable CA1062 // Validate arguments of public methods
        new Uri(ollamaOptions.Value.OllamaApiPullRelativeUrl, UriKind.Relative),
#pragma warning restore CA1062 // Validate arguments of public methods
        new Uri(ollamaOptions.Value.OllamaApiEmbeddingsRelativeUrl, UriKind.Relative),
        ollamaOptions.Value.EmbeddingsLanguageModelName)
    {
        // Logger settings are read from appsettings.json or appsettings.Development.json depending on the environment.
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _http_client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        ArgumentNullException.ThrowIfNull(ollamaOptions, nameof(ollamaOptions));
        _ollama_embeddings_relative_url = new Uri(ollamaOptions.Value.OllamaApiEmbeddingsRelativeUrl, UriKind.Relative);
    }

    public override async Task<VectorEmbeddings?> GenerateAsync(Stream responseBody, CancellationToken cancellationToken)
    {
        return await JsonSerializer.DeserializeAsync<VectorEmbeddings>(
        responseBody,
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        },
        cancellationToken);
    }
}
