using System.Text;
using System.Text.Json;
using Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static System.Net.Mime.MediaTypeNames;

public class ResponseLanguageModel : LanguageModel<LanguageResponse>
{
    private readonly ILogger _logger;
    private readonly OllamaOptions _ollamaOptions;

    public ResponseLanguageModel(ILogger logger, IOptions<OllamaOptions> ollamaOptions)
    : base(logger, new Uri(ollamaOptions.Value.OllamaApiBaseUrl), new Uri(ollamaOptions.Value.OllamaApiPullRelativeUrl, UriKind.Relative), ollamaOptions.Value.ResponseLanguageModelName)
    {
        // Logger settings are read from appsettings.json or appsettings.Development.json depending on the environment.
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _ollamaOptions = ollamaOptions?.Value ?? throw new ArgumentNullException(nameof(ollamaOptions));
    }

    public override async Task<LanguageResponse> Generate(Uri ollamaApiBaseUrl, string languageModelName, string text, CancellationToken cancellationToken)
    {
        // TODO: Some of this code can go in base class as shared logic, returning Http response message only.
        using (var client = new HttpClient())
        {
            var api_url = new Uri(ollamaApiBaseUrl, _ollamaOptions.OllamaApiResponseRelativeUrl);
            _logger.LogTrace($"Generating response using model {languageModelName}, url {api_url}.");

            // Keep streaming to false for now, enable it later.
            var api_payload = JsonSerializer.Serialize(new { model = languageModelName, stream = false, prompt = text });

            using (var content = new StringContent(api_payload, Encoding.UTF8, Application.Json))
            {
                var response = await client.PostAsync(api_url, content, cancellationToken: cancellationToken);
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
}
