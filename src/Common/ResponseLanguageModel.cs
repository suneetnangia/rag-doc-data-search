using Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ResponseLanguageModel : LanguageModel<string>
{
    private readonly OllamaOptions _ollamaOptions;

    public ResponseLanguageModel(ILogger logger, IOptions<OllamaOptions> ollamaOptions)
    : base(logger, new Uri(ollamaOptions.Value.OllamaApiBaseUrl), new Uri(ollamaOptions.Value.OllamaApiPullRelativeUrl, UriKind.Relative), ollamaOptions.Value.ResponseLanguageModelName)
    {
        _ollamaOptions = ollamaOptions?.Value ?? throw new ArgumentNullException(nameof(ollamaOptions));
        throw new NotImplementedException();
    }

    public override async Task<string> Generate(Uri ollamaBaseUrl, string languageModelName, string text)
    {
        throw new NotImplementedException();
    }
}
