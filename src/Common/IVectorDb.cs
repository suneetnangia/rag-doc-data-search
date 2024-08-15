using Common;

public interface IVectorDb
{
    Task AddDocumentAsync(LanguageModel<VectorEmbeddings> embeddingsLanguageModel, string documentId, string document, CancellationToken cancellationToken);
    Task<IEnumerable<VectorDocument>> GetDocumentsAsync(LanguageModel<VectorEmbeddings> embeddingsLanguageModel, LanguageModel<string> responseLanguageModel, string searchString, CancellationToken cancellationToken, int maxResults = 1);
}
