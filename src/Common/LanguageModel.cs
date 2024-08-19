namespace Common;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

/// <summary>
/// Represents a base for large or small language model in Gen AI context.
/// </summary>
public abstract class LanguageModel<T>
{
    private readonly ILogger _logger;
    private readonly Uri _ollama_api_base_url;
    private readonly Uri _ollama_api_relative_url;
    private readonly string _language_model_name;

    /// <summary>
    /// Creates an instance of LanguageModel with a specific model passed in.
    /// </summary>
    /// <param name="name"></param>
    public LanguageModel(ILogger logger, Uri ollamaApiBaseUrl, Uri ollamaApiRelativeUrl, string languageModelName)
    {
        // Logger settings are read from appsettings.json or appsettings.Development.json depending on the environment.
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _ollama_api_base_url = ollamaApiBaseUrl ?? throw new ArgumentNullException(nameof(ollamaApiBaseUrl));
        _language_model_name = languageModelName ?? throw new ArgumentNullException(nameof(languageModelName));
        _ollama_api_relative_url = ollamaApiRelativeUrl ?? throw new ArgumentNullException(nameof(ollamaApiRelativeUrl));

        // Downloads and runs the configured language model.
        Load().Wait();
    }

    private async Task Load()
    {
        using (var client = new HttpClient())
        {
            var api_url = new Uri(_ollama_api_base_url, _ollama_api_relative_url);           
            var api_payload = JsonSerializer.Serialize(new { name = _language_model_name });

            _logger.LogTrace($"Loading model {_language_model_name}, using url {api_url}.");

            using (var content = new StringContent(api_payload, Encoding.UTF8, Application.Json))
            {
                var response = await client.PostAsync(api_url, content);

                // TODO: Http 200 does not always mean model is loaded successfully e.g. invalid model name returns 200 as well.
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Model {_language_model_name} loaded successfully. {response}");
                }
                else
                {
                    throw new ApplicationException($"Failed to load language model {_language_model_name}, {response}.");
                }
            }
        }
    }

    /// <summary>
    /// Generates response or embeddings using the language model.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public Task<T> Generate(string text)
    {
        return Generate(_ollama_api_base_url, _language_model_name, text);
    }

    /// <summary>
    /// Implemented by derived classes which generates human readable response or embeddings using the language model.
    /// </summary>
    /// <param name="ollamaBaseUrl"></param>
    /// <param name="languageModelName"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public abstract Task<T> Generate(Uri ollamaBaseUrl, string languageModelName, string text);

    public void Unload()
    {
        // Unload the model.
        throw new NotImplementedException();
    }
}
