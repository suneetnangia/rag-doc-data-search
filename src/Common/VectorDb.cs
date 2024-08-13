namespace Common;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;

/// <summary>
/// Represents a vector database.
/// </summary>
public class VectorDb
{
    private readonly ILogger _logger;

    private readonly ChromaClient _chroma_client;

    public VectorDb(ILogger logger, string connectionString)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // TODO: Currently implement Chroma Db but should be abstracted to support other databases.
        _chroma_client = new ChromaClient(connectionString);
    }

    /// <summary>
    /// Inserts embeddings into the database
    /// TODO: Only pass specific interface of LanguageModel for creating embeddings.
    /// </summary>
    public async Task AddDocumentAsync(string vectorName, LanguageModel embeddingsLanguageModel, string documentId, string document)
    {
        if (string.IsNullOrEmpty(vectorName))
        {
           throw new ArgumentNullException(nameof(vectorName));
        }

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

        // TODO Add cancellation tokens in async calls.
        await _chroma_client.CreateCollectionAsync(vectorName);

        var generated_embeddings = await embeddingsLanguageModel.GenerateEmbeddings(document);        
        var embedding = new ReadOnlyMemory<float>(generated_embeddings.Embedding);
        var embeddings = new ReadOnlyMemory<float>[] { embedding };
        var embedding_ids = new string[] { documentId };

        var collection = await _chroma_client.GetCollectionAsync(vectorName);
        await _chroma_client.UpsertEmbeddingsAsync(collection.Id, embedding_ids, embeddings, null, CancellationToken.None);

        _logger.LogInformation($"Inserted embeddings in vector db, document id: {documentId}");
    }

    public async Task<IEnumerable<Document>> GetDocumentsAsync(string vectorName, LanguageModel embeddingsLanguageModel, LanguageModel languageModel, string searchString, int maxResults = 1)
    {
        if (string.IsNullOrEmpty(vectorName))
        {
           throw new ArgumentNullException(nameof(vectorName));
        }

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

        var generated_embeddings = await embeddingsLanguageModel.GenerateEmbeddings(searchString);
        var embedding = new ReadOnlyMemory<float>(generated_embeddings.Embedding);
        var embeddings = new ReadOnlyMemory<float>[] { embedding };

        var documents = await _chroma_client.QueryEmbeddingsAsync(vectorName, embeddings, maxResults, null, CancellationToken.None);

        return new List<Document> { new Document
        {
            Id = "Id",
            Text = "document.Text"
        }};
    }
}
