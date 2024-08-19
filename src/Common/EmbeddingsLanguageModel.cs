namespace Common;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static System.Net.Mime.MediaTypeNames;

/// <summary>
/// Represents a model that generates embeddings for vector db.
/// </summary>
public class EmbeddingsLanguageModel : LanguageModel<VectorEmbeddings>
{
    private readonly ILogger _logger;

    private readonly OllamaOptions _ollamaOptions;
    
    public EmbeddingsLanguageModel(ILogger logger, IOptions<OllamaOptions> ollamaOptions)
    : base(logger, new Uri(ollamaOptions.Value.OllamaApiBaseUrl), new Uri(ollamaOptions.Value.OllamaApiPullRelativeUrl, UriKind.Relative), ollamaOptions.Value.EmbeddingsLanguageModelName)
    {
        // Logger settings are read from appsettings.json or appsettings.Development.json depending on the environment.
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _ollamaOptions = ollamaOptions?.Value ?? throw new ArgumentNullException(nameof(ollamaOptions));        
    }

    public override async Task<VectorEmbeddings> Generate(Uri ollamaApiBaseUrl, string languageModelName, string text, CancellationToken cancellationToken)
    {
        // TODO: Some of this code can go in base class as shared logic, returning Http response message.
        using (var client = new HttpClient())
        {
            var api_url = new Uri(ollamaApiBaseUrl, _ollamaOptions.OllamaApiEmbeddingsRelativeUrl);
            _logger.LogTrace($"Generating embeddings using model {languageModelName}, url {api_url}.");

            var api_payload = JsonSerializer.Serialize(new { model = languageModelName, prompt = text });

            using (var content = new StringContent(api_payload, Encoding.UTF8, Application.Json))
            {
                var response = await client.PostAsync(api_url, content, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    var embeddings = JsonSerializer.Deserialize<VectorEmbeddings>(responseBody, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? throw new NullReferenceException($"Failed deserializing embeddings using model {languageModelName}, {responseBody}.");

                    _logger.LogTrace($"Embeddings generated using model {languageModelName}, dimension count {embeddings.Embedding.Length}.");

                    return embeddings;
                }
                else
                {
                    throw new ApplicationException($"Failed to generate embeddings using model {languageModelName}, {response}.");
                }
            }
        }
    }
}
