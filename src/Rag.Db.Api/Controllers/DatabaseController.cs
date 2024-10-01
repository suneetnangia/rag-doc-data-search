namespace Rag.Db.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Rag.Common.Database;
using Rag.Common.LanguageModel;
using Rag.Common.Responses;
using Rag.Common.VectorDb;

[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly ILogger<DatabaseController> _logger;
    private readonly IVectorDb _vectorDb;
    private readonly Model<VectorEmbeddings> _embeddingsLanguageModel;
    private readonly Model<LanguageResponse> _responseLanguageModel;
    private readonly InfluxDbRepository _influxDbRepository;

    public DatabaseController(
        ILogger<DatabaseController> logger,
        IVectorDb vectorDb,
        Model<VectorEmbeddings> embeddingsLanguageModel,
        Model<LanguageResponse> responseLanguageModel,
        InfluxDbRepository influxDbRepository)
    {
        // Logger settings are read from appsettings.json or appsettings.Development.json depending on the environment.
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _vectorDb = vectorDb ?? throw new ArgumentNullException(nameof(vectorDb));
        _embeddingsLanguageModel = embeddingsLanguageModel ?? throw new ArgumentNullException(nameof(embeddingsLanguageModel));
        _responseLanguageModel = responseLanguageModel ?? throw new ArgumentNullException(nameof(responseLanguageModel));
        _influxDbRepository = influxDbRepository ?? throw new ArgumentNullException(nameof(influxDbRepository));
    }

    [HttpGet]
    public async Task<IActionResult> Get(string queryString, bool useLanguageResponse = false, float minResultScore = 0.5f)
    {
        if (string.IsNullOrEmpty(queryString))
        {
            throw new ArgumentNullException(nameof(queryString));
        }

        _logger.LogTrace($"Retrieving documents using search string '{queryString}'");

        // Get a matching query from the vector database.
        var queryResponse = await _vectorDb.GetDataAsync(
            _embeddingsLanguageModel,
            useLanguageResponse ? _responseLanguageModel : null,
            _influxDbRepository,
            queryString,
            HttpContext.RequestAborted,
            minResultScore);

        // Return Http 200 OK with the documents.
        return Ok(queryResponse);
    }

    [HttpPost]
    public async Task<IActionResult> Post(string[] queries)
    {
        if (queries is null)
        {
            throw new ArgumentNullException(nameof(queries));
        }

        // TODO: This can be optimized by parallel processing of queries.
        foreach (var query in queries)
        {
            _logger.LogTrace($"Adding query: {query}");

            // We are using GUIDs for document ids but it can be changed to something else later
            // for better ordering of responses and disk space etc..
            var documentId = Guid.NewGuid();

            await _vectorDb.AddDocumentAsync(
                _embeddingsLanguageModel,
                documentId,
                query,
                HttpContext.RequestAborted);
        }

        return Ok();
    }
}
