namespace Rag.Common.VectorDb;

using Rag.Common.Database;
using Rag.Common.LanguageModel;
using Rag.Common.Responses;

public interface IVectorDb
{
    Task AddDocumentAsync(Model<VectorEmbeddings> embeddingsLanguageModel, Guid documentId, BaseVectorDbRecord document, CancellationToken cancellationToken);

    Task<IEnumerable<Task<SearchResponse>>> GetDocumentsAsync(Model<VectorEmbeddings> embeddingsLanguageModel, Model<LanguageResponse>? responseLanguageModel, string searchString, CancellationToken cancellationToken, float minResultScore, ulong maxResults);

    Task<DataQueryResponse?> GetDataAsync(Model<VectorEmbeddings> embeddingsLanguageModel, Model<LanguageResponse>? responseLanguageModel, InfluxDbRepository influxDbRepository, string queryString, CancellationToken cancellationToken, float minResultScore);
}
