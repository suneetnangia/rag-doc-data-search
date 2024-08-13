namespace Doc.Api.Controllers;

using Common;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class DocumentController : ControllerBase
{
    private readonly Uri _ollama_base_url;
    private readonly string _chroma_db_connection_string;
    private readonly string _language_model;
    private readonly string _embeddings_language_model;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(ILogger<DocumentController> logger)
    {
        _logger = logger;

        // TODO: Inject this via DI.
        _ollama_base_url = new Uri("http://localhost:11434/api/");
        _chroma_db_connection_string = "http://localhost:8080";
        _language_model = "phi3:mini";
        _embeddings_language_model = "mxbai-embed-large";
    }

    [HttpGet(Name = "GetDocuments")]
    public async Task<IEnumerable<Document>> Get(string searchString, int maxResults = 1)
    {
        if (string.IsNullOrEmpty(searchString))
        {
            throw new ArgumentNullException(nameof(searchString));
        }

        var vectorDb = new VectorDb(_logger, _chroma_db_connection_string);
        var documents = await vectorDb.GetDocumentsAsync(
            "machine_manuals",
             new LanguageModel(_logger, _ollama_base_url, _language_model),
             //  new LanguageModel(_logger, _ollama_base_url, _embeddings_language_model),
             new LanguageModel(_logger, _ollama_base_url, _language_model),
             searchString,
             maxResults);

        return documents;
    }

    [HttpPost(Name = "InsertDocuments")]
    public async Task Post(string[] documents)
    {
        if (documents is null)
        {
            throw new ArgumentNullException(nameof(documents));
        }

        var vectorDb = new VectorDb(_logger, _chroma_db_connection_string);
        foreach (var document in documents)
        {
            _logger.LogInformation($"Inserting document: {document}");

            // We are using GUIDs for document ids but it can be changed to something else later for better ordering of responses and disk space etc..
            // TODO: fix bug where use of _embeddings_language_model caused exception when it's embeddings are inserted in ChromoDb.
            await vectorDb.AddDocumentAsync(
                "machine_manuals",
                // new LanguageModel(_logger, _ollama_base_url, _embeddings_language_model),
                new LanguageModel(_logger, _ollama_base_url, _language_model),
                Guid.NewGuid().ToString(),
                document);
        }
    }
}