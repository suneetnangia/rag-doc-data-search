namespace Common;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

/// <summary>
/// Represents a specific implementation of vector database.
/// </summary>
public class QdrantVectorDb : IVectorDb
{
    private readonly ILogger _logger;

    private readonly string _vectorDbCollectionName;

    // private readonly QdrantVectorStore _vector_db_store;

    private readonly QdrantClient _vector_db_client;

    public QdrantVectorDb(ILogger logger, IOptions<VectorDbOptions> vectorDbOptions)
    {
        // Logger settings and other options are read from appsettings.json or appsettings.Development.json depending on the environment.
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _vectorDbCollectionName = vectorDbOptions?.Value.CollectionName ?? throw new ArgumentNullException(nameof(vectorDbOptions));
        var vectorDbHostName = vectorDbOptions?.Value.HostName ?? throw new ArgumentNullException(nameof(vectorDbOptions));
        var vectorDbPort = vectorDbOptions?.Value.HostPort ?? throw new ArgumentNullException(nameof(vectorDbOptions));

        _vector_db_client = new QdrantClient(vectorDbHostName, vectorDbPort);
    }

    /// <summary>
    /// Initialize the vector database.
    /// </summary>
    /// <returns></returns>
    public async Task Init()
    {
        _logger.LogInformation($"Initializing vector db...");

        if (await _vector_db_client.CollectionExistsAsync(_vectorDbCollectionName))
        {
            _logger.LogTrace($"Collection {_vectorDbCollectionName} already exists, making use of it.");
        }
        else
        {
            _logger.LogInformation($"Collection {_vectorDbCollectionName} does not exists, creating it.");
            await _vector_db_client.CreateCollectionAsync(collectionName: _vectorDbCollectionName, vectorsConfig: new VectorParams
            {
                // TODO: Vector dimension and size can be read from configuration or per emebdding model.
                Size = 1024,
                Distance = Distance.Cosine
            });
        }
    }

    /// <summary>
    /// Add document into the vector database.
    /// </summary>
    public async Task AddDocumentAsync(LanguageModel<VectorEmbeddings> embeddingsLanguageModel, Guid documentId, string document, CancellationToken cancellationToken)
    {
        if (embeddingsLanguageModel is null)
        {
            throw new ArgumentNullException(nameof(embeddingsLanguageModel));
        }

        if (documentId == Guid.Empty)
        {
            // TODO: Guid is a value type, so it cannot be null.
            throw new ArgumentNullException(nameof(documentId));
        }

        if (string.IsNullOrEmpty(document))
        {
            throw new ArgumentNullException(nameof(document));
        }

        var generated_embeddings = await embeddingsLanguageModel.Generate(document);

        var result = await _vector_db_client.UpsertAsync(_vectorDbCollectionName, new List<PointStruct>
            {
                new()
                {
                    Id = documentId,
                    Vectors = generated_embeddings.Embedding,
                    Payload = {
                        // TODO: This can be optimized by defining schema/strong type for the payload which can include any key-value pairs.
                        ["document"] = document,
                    }
                }
            });

        _logger.LogTrace($"Inserted embeddings in vector db, document : {result}");
    }

    public async Task<IEnumerable<VectorDocument>> GetDocumentsAsync(LanguageModel<VectorEmbeddings> embeddingsLanguageModel, LanguageModel<VectorDocument> languageModel, string searchString, CancellationToken cancellationToken, float minResultScore = 0.5f, ulong maxResults = 1)
    {
        if (embeddingsLanguageModel is null)
        {
            throw new ArgumentNullException(nameof(embeddingsLanguageModel));
        }

        if (languageModel is null)
        {
            throw new ArgumentNullException(nameof(languageModel));
        }

        if (string.IsNullOrEmpty(searchString))
        {
            throw new ArgumentNullException(nameof(searchString));
        }

        var generated_embeddings = await embeddingsLanguageModel.Generate(searchString);
        var embedding = new ReadOnlyMemory<float>(generated_embeddings.Embedding);

        var total_documents = await _vector_db_client.CountAsync(_vectorDbCollectionName);
        var documents = await _vector_db_client.SearchAsync(_vectorDbCollectionName, embedding, limit: maxResults, scoreThreshold: minResultScore, payloadSelector: true);

        var vectorDocuments = documents.Select(doc => new VectorDocument
        {
            Id = doc.Id.Uuid,
            Score = doc.Score,
            Text = doc.Payload.ToString()
        }).ToList();

        _logger.LogTrace($"Found {vectorDocuments.Count} documents in vector db which has total of {total_documents} documents.");

        return vectorDocuments;
    }
}
