using Common;

public interface IVectorDb
{
    Task AddDocumentAsync(LanguageModel<VectorEmbeddings> embeddingsLanguageModel, Guid documentId, BaseVectorDbRecord document, CancellationToken cancellationToken);
    Task<IEnumerable<Task<SearchResponse?>>> GetDocumentsAsync(LanguageModel<VectorEmbeddings> embeddingsLanguageModel, LanguageModel<LanguageResponse>? responseLanguageModel, string searchString, CancellationToken cancellationToken, float minResultScore, ulong maxResults);
    Task<DataQueryResponse?> GetDataAsync(LanguageModel<VectorEmbeddings> embeddingsLanguageModel, LanguageModel<LanguageResponse>? responseLanguageModel, InfluxDbRepository influxDbRepository, string queryString, CancellationToken cancellationToken, float minResultScore);
}
