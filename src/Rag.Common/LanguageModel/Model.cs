namespace Rag.Common.LanguageModel;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

public abstract class Model<T>
{
    private readonly ILogger _logger;
    private readonly HttpClient _http_client;
    private readonly Uri _ollama_api_pull_relative_url;
    private readonly Uri _ollama_api_relative_url;
    private readonly string _language_model_name;

    public Model(ILogger logger, HttpClient httpClient, Uri ollamaApiPullRelativeUrl, Uri ollamaApiRelativeUrl, string languageModelName)
    {
        // Logger settings are read from appsettings.json or appsettings.Development.json depending on the environment.
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _http_client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _language_model_name = languageModelName ?? throw new ArgumentNullException(nameof(languageModelName));
        _ollama_api_pull_relative_url = ollamaApiPullRelativeUrl ?? throw new ArgumentNullException(nameof(ollamaApiPullRelativeUrl));
        _ollama_api_relative_url = ollamaApiRelativeUrl ?? throw new ArgumentNullException(nameof(ollamaApiRelativeUrl));

        // Downloads and runs the configured language model.
        Load().Wait();
    }

    public virtual async Task<T> Generate(string text, CancellationToken cancellationToken)
    {
        _logger.LogTrace($"Generating embeddings using model {_language_model_name}, base url {_http_client.BaseAddress}, relative url {_ollama_api_relative_url}.");

        var api_payload = JsonSerializer.Serialize(new { model = _language_model_name, stream = false, prompt = text });

        using (var content = new StringContent(api_payload, Encoding.UTF8, Application.Json))
        {
            var response = await _http_client.PostAsync(_ollama_api_relative_url, content, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var response_body = await response.Content.ReadAsStreamAsync(cancellationToken);

                var model_response = await GenerateAsync(response_body, cancellationToken)
                ?? throw new NullReferenceException($"Failed to generate response using model {_language_model_name}.");

                _logger.LogTrace($"Model response generated using model {_language_model_name}.");

                return model_response;
            }
            else
            {
                throw new ApplicationException($"Failed to generate embeddings using model {_language_model_name}, {response}.");
            }
        }
    }

    public abstract Task<T?> GenerateAsync(Stream responseBody, CancellationToken cancellationToken);

    public void Unload()
    {
        // Unload the model.
        throw new NotImplementedException();
    }

    private async Task Load()
    {
        var api_payload = JsonSerializer.Serialize(new { name = _language_model_name });

        _logger.LogTrace($"Loading model {_language_model_name}, using relative url {_ollama_api_pull_relative_url}.");

        using (var content = new StringContent(api_payload, Encoding.UTF8, Application.Json))
        {
            var response = await _http_client.PostAsync(_ollama_api_pull_relative_url, content);

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
