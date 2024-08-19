namespace Doc.Api.Controllers;

using Common;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class DocumentController : ControllerBase
{
    private readonly ILogger<DocumentController> _logger;
    private readonly IVectorDb _vectorDb;
    private readonly LanguageModel<VectorEmbeddings> _embeddingsLanguageModel;
    private readonly LanguageModel<VectorDocument> _responseLanguageModel;

    public DocumentController(
        ILogger<DocumentController> logger,
        IVectorDb vectorDb,
        LanguageModel<VectorEmbeddings> embeddingsLanguageModel,
        LanguageModel<VectorDocument> responseLanguageModel)
    {
        // Logger settings are read from appsettings.json or appsettings.Development.json depending on the environment.        
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _vectorDb = vectorDb ?? throw new ArgumentNullException(nameof(vectorDb));
        _embeddingsLanguageModel = embeddingsLanguageModel ?? throw new ArgumentNullException(nameof(embeddingsLanguageModel));
        _responseLanguageModel = responseLanguageModel ?? throw new ArgumentNullException(nameof(responseLanguageModel));
    }

    [HttpGet]
    public async Task<IActionResult> Get(string searchString, float minResultScore = 0.5f, ulong maxResults = 1)
    {
        if (string.IsNullOrEmpty(searchString))
        {
            throw new ArgumentNullException(nameof(searchString));
        }

        if (maxResults < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxResults), "Max results must be greater than 0.");
        }

        _logger.LogTrace($"Retrieving documents using search string '{searchString}'");

        var documents = await _vectorDb.GetDocumentsAsync(
            _embeddingsLanguageModel,
            _responseLanguageModel,
            searchString,
            CancellationToken.None,
            minResultScore,
            maxResults);

        // Return Http 200 OK with the documents.
        return Ok(documents);
    }

    [HttpPost]
    public async Task<IActionResult> Post(string[] documents)
    {
        if (documents is null)
        {
            throw new ArgumentNullException(nameof(documents));
        }

        // TODO: This can be optimized by parallel processing of documents.
        foreach (var document in documents)
        {
            _logger.LogTrace($"Adding document: {document}");

            // We are using GUIDs for document ids but it can be changed to something else later
            // for better ordering of responses and disk space etc..
            var documentId = Guid.NewGuid();

            await _vectorDb.AddDocumentAsync(
                _embeddingsLanguageModel,
                documentId,
                document,
                CancellationToken.None);
        }

        return Ok();
    }
}
