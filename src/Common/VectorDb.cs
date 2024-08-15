namespace Common;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Connectors.Chroma;

/// <summary>
/// Represents a vector database, Chroma DB in this implementation.
/// </summary>
public class VectorDb : IVectorDb
{
    private readonly ILogger _logger;

    private readonly VectorDbOptions _vectorDbOptions;

    private readonly ChromaClient _chroma_client;

    public VectorDb(ILogger logger, IOptions<VectorDbOptions> vectorDbOptions)
    {
        // Logger settings are read from appsettings.json or appsettings.Development.json depending on the environment.
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Mixing embeddings from different language models in the same collection can be problematic.
        _vectorDbOptions = vectorDbOptions?.Value ?? throw new ArgumentNullException(nameof(vectorDbOptions));
        _chroma_client = new ChromaClient(_vectorDbOptions.ConnectionString);
    }

    /// <summary>
    /// Add document into the vector database.    
    /// </summary>
    public async Task AddDocumentAsync(LanguageModel<VectorEmbeddings> embeddingsLanguageModel, string documentId, string document, CancellationToken cancellationToken)
    {
        if (embeddingsLanguageModel is null)
        {
            throw new ArgumentNullException(nameof(embeddingsLanguageModel));
        }

        if (string.IsNullOrEmpty(documentId))
        {
            throw new ArgumentNullException(nameof(documentId));
        }

        if (string.IsNullOrEmpty(document))
        {
            throw new ArgumentNullException(nameof(document));
        }

        // Create collection if it does not exist, no exception if it does exist.
        await _chroma_client.CreateCollectionAsync(_vectorDbOptions.CollectionName);

        var generated_embeddings = await embeddingsLanguageModel.Generate(document);
        var embedding = new ReadOnlyMemory<float>(generated_embeddings.Embedding);
        var embeddings = new ReadOnlyMemory<float>[] { embedding };
        var embedding_ids = new string[] { documentId };

        var collection = await _chroma_client.GetCollectionAsync(_vectorDbOptions.CollectionName, cancellationToken) ??
                         throw new Exception($"Vector Db collection {_vectorDbOptions.CollectionName} not found.");

        await _chroma_client.UpsertEmbeddingsAsync(collection.Id, embedding_ids, embeddings, null, cancellationToken);

        _logger.LogTrace($"Inserted embeddings in vector db, document id: {documentId}");
    }

    public async Task<IEnumerable<VectorDocument>> GetDocumentsAsync(LanguageModel<VectorEmbeddings> embeddingsLanguageModel, LanguageModel<string> languageModel, string searchString, CancellationToken cancellationToken, int maxResults = 1)
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
        var embeddings = new ReadOnlyMemory<float>[] { embedding };

        // TODO: This is throwing Http 400 from ChromaDb, investigate.
        var documents = await _chroma_client.QueryEmbeddingsAsync(_vectorDbOptions.CollectionName, embeddings, maxResults, ["documents"], cancellationToken);

        return new List<VectorDocument> { new VectorDocument
        {
            Id = "Id",
            Text = "document.Text"
        }};
    }
}
