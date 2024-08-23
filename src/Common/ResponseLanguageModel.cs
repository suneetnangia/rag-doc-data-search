namespace Common;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static System.Net.Mime.MediaTypeNames;

public class ResponseLanguageModel : LanguageModel<LanguageResponse>
{
    private readonly ILogger _logger;
    private readonly HttpClient _http_client;
    private readonly Uri _ollama_api_response_relative_url;

    public ResponseLanguageModel(ILogger logger, HttpClient httpClient, IOptions<OllamaOptions> ollamaOptions)
    : base(logger, httpClient, new Uri(ollamaOptions.Value.OllamaApiPullRelativeUrl, UriKind.Relative), ollamaOptions.Value.ResponseLanguageModelName)
    {
        // Logger settings are read from appsettings.json or appsettings.Development.json depending on the environment.
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _http_client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        if (ollamaOptions is null)
        {
            throw new ArgumentNullException(nameof(ollamaOptions));
        }

        _ollama_api_response_relative_url = new Uri(ollamaOptions.Value.OllamaApiResponseRelativeUrl, UriKind.Relative);
    }

    public override async Task<LanguageResponse> Generate(string languageModelName, string text, CancellationToken cancellationToken)
    {
        // TODO: Some of this code can go in base class as shared logic, returning Http response message only.        
        _logger.LogTrace($"Generating response using model {languageModelName}, base url {_http_client.BaseAddress}, relative url {_ollama_api_response_relative_url}.");

        // Keep streaming to false for now, enable it later.
        var api_payload = JsonSerializer.Serialize(new { model = languageModelName, stream = false, prompt = text });

        using (var content = new StringContent(api_payload, Encoding.UTF8, Application.Json))
        {
            var response = await _http_client.PostAsync(_ollama_api_response_relative_url, content, cancellationToken: cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();

                var language_response = JsonSerializer.Deserialize<LanguageResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = false,
                }) ?? throw new NullReferenceException($"Failed deserializing language response using model '{languageModelName}', {responseBody}.");

                _logger.LogTrace($"Response generated using model '{languageModelName}'.");

                return language_response;
            }
            else
            {
                throw new ApplicationException($"Failed to generate response using model '{languageModelName}', {response}.");
            }
        }
    }
}
