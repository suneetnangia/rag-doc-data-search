using Common;

public interface IVectorDb
{
    Task AddDocumentAsync(LanguageModel<VectorEmbeddings> embeddingsLanguageModel, Guid documentId, string document, CancellationToken cancellationToken);
    Task<IEnumerable<VectorDocument>> GetDocumentsAsync(LanguageModel<VectorEmbeddings> embeddingsLanguageModel, LanguageModel<VectorDocument> responseLanguageModel, string searchString, CancellationToken cancellationToken, float minResultScore, ulong maxResults);
}
