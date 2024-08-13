namespace Common;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

/// <summary>
/// Represents a large or small language model in Gen AI world.
/// </summary>
public class LanguageModel
{
    private readonly Uri _ollama_base_url;
    private readonly string _language_model_name;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates an instance of LanguageModel with a specific model passed in.
    /// </summary>
    /// <param name="name"></param>
    public LanguageModel(ILogger logger, Uri ollamaBaseUrl, string name)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ollama_base_url = ollamaBaseUrl ?? throw new ArgumentNullException(nameof(ollamaBaseUrl));
        _language_model_name = name ?? throw new ArgumentNullException(nameof(name));

        Load();
    }

    private async void Load()
    {
        using (var client = new HttpClient())
        {
            var api_url = new Uri(_ollama_base_url, "pull");
            var api_payload = $"{{\"name\": \"{_language_model_name}\"}}";

            using (var content = new StringContent(api_payload, Encoding.UTF8, "application/json"))
            {
                var response = await client.PostAsync(api_url, content);

                // TODO: Http 200 does not always mean model is loaded successfully e.g. use invalid model name returns 200.
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Model {_language_model_name} loaded successfully. {response}");
                }
                else
                {
                    throw new Exception($"Failed loading model {_language_model_name}, {response}.");
                }
            }
        }
    }

    public void Unload()
    {
        // Unload the model.
        throw new NotImplementedException();
    }

    public async Task<LocalEmbeddings> GenerateEmbeddings(string document)
    {
        using (var client = new HttpClient())
        {
            var api_url = new Uri(_ollama_base_url, "embeddings");
            var api_payload = $"{{\"model\": \"{_language_model_name}\", \"prompt\": \"{document}\"}}";

            using (var content = new StringContent(api_payload, Encoding.UTF8, "application/json"))
            {
                var response = await client.PostAsync(api_url, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    var embeddings = JsonSerializer.Deserialize<LocalEmbeddings>(responseBody, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? throw new NullReferenceException($"Failed deserializing embeddings using model {_language_model_name}, {responseBody}.");

                    _logger.LogInformation($"Embeddings generated using model {_language_model_name}, count {embeddings.Embedding.Length}.");

                    return embeddings;
                }
                else
                {
                    throw new Exception($"Failed generating embeddings using model {_language_model_name}, {response}.");
                }
            }
        }
    }
}
