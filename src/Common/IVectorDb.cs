using Common;

public interface IVectorDb
{
    Task AddDocumentAsync(LanguageModel<VectorEmbeddings> embeddingsLanguageModel, Guid documentId, string document, CancellationToken cancellationToken);
    Task<IEnumerable<Task<SearchResponse>>> GetDocumentsAsync(LanguageModel<VectorEmbeddings> embeddingsLanguageModel, LanguageModel<LanguageResponse>? responseLanguageModel, string searchString, CancellationToken cancellationToken, float minResultScore, ulong maxResults);
}
