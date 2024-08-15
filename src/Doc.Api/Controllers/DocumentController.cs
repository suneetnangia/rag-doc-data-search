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
    private readonly LanguageModel<string> _responseLanguageModel;

    public DocumentController(
        ILogger<DocumentController> logger,
        IVectorDb vectorDb,
        LanguageModel<VectorEmbeddings> embeddingsLanguageModel,
        LanguageModel<string> responseLanguageModel)
    {
        // Logger settings are read from appsettings.json or appsettings.Development.json depending on the environment.        
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _vectorDb = vectorDb ?? throw new ArgumentNullException(nameof(vectorDb));
        _embeddingsLanguageModel = embeddingsLanguageModel ?? throw new ArgumentNullException(nameof(embeddingsLanguageModel));
        _responseLanguageModel = responseLanguageModel ?? throw new ArgumentNullException(nameof(responseLanguageModel));
    }

    [HttpGet(Name = "GetDocuments")]
    public async Task<IEnumerable<VectorDocument>> Get(string searchString, int maxResults = 1)
    {
        if (string.IsNullOrEmpty(searchString))
        {
            throw new ArgumentNullException(nameof(searchString));
        }

        _logger.LogTrace($"Retrieving documents using search string '{searchString}'");

        var documents = await _vectorDb.GetDocumentsAsync(
            _embeddingsLanguageModel,
            _responseLanguageModel,
            searchString,
            CancellationToken.None,
            maxResults);

        return documents;
    }

    [HttpPost(Name = "AddDocuments")]
    public async Task Post(string[] documents)
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
            var documentId = Guid.NewGuid().ToString();

            await _vectorDb.AddDocumentAsync(
                _embeddingsLanguageModel,
                documentId,
                document,
                CancellationToken.None);
        }
    }
}
