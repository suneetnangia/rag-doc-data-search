namespace Rag.Common.VectorDb;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Rag.Common.Database;
using Rag.Common.LanguageModel;
using Rag.Common.Responses;

/// <summary>
/// Represents a specific implementation of vector database.
/// </summary>
public class QdrantVectorDb : IVectorDb
{
    private readonly ILogger _logger;

    private readonly string _vectorDbCollectionName;

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

    public async Task Init(CancellationToken cancellationToken = default(CancellationToken))
    {
        _logger.LogInformation($"Initializing vector db...");

        if (await _vector_db_client.CollectionExistsAsync(_vectorDbCollectionName, cancellationToken: cancellationToken))
        {
            _logger.LogTrace($"Collection {_vectorDbCollectionName} already exists, making use of it...");
        }
        else
        {
            _logger.LogInformation($"Collection {_vectorDbCollectionName} does not exists, creating it...");
            await _vector_db_client.CreateCollectionAsync(
                collectionName: _vectorDbCollectionName,
                vectorsConfig: new VectorParams
                {
                    // TODO: Vector dimension and size can be read from configuration or inferred from the embedding model.
                    Size = 1024,
                    Distance = Distance.Cosine
                },
                cancellationToken: cancellationToken);
        }
    }

    public async Task AddDocumentAsync(
        Model<VectorEmbeddings> embeddingsLanguageModel,
        Guid documentId,
        VectorDbRecord document,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(embeddingsLanguageModel, nameof(embeddingsLanguageModel));
        ArgumentNullException.ThrowIfNull(document, nameof(document));

        if (documentId == Guid.Empty)
        {
            throw new ArgumentOutOfRangeException(nameof(documentId));
        }

        var generated_embeddings = await embeddingsLanguageModel.Generate(document.Document, cancellationToken);

        var documentPointStruct = new PointStruct
        {
            Id = documentId,
            Vectors = generated_embeddings.Embedding,
        };

        switch (document)
        {
            case DocumentVectorDbRecord docRecord:
            documentPointStruct.Payload.Add(docRecord.ConvertToMapField());
            break;
            case DataVectorDbRecord dataRecord:
            documentPointStruct.Payload.Add(dataRecord.ConvertToMapField());
            break;
            default:
            throw new InvalidOperationException("Document type is not supported.");
        }

        var result = await _vector_db_client.UpsertAsync(
            _vectorDbCollectionName,
            new List<PointStruct>
            {
                documentPointStruct
            },
            cancellationToken: cancellationToken);

        _logger.LogTrace($"Inserted embeddings in vector db, document : {result}");
    }

    public async Task<IEnumerable<Task<Common.SearchResponse>>> GetDocumentsAsync(
        Model<VectorEmbeddings> embeddingsLanguageModel,
        Model<LanguageResponse>? responseLanguageModel,
        string searchString,
        CancellationToken cancellationToken,
        float minResultScore = 0.5f,
        ulong maxResults = 1)
    {
        ArgumentNullException.ThrowIfNull(embeddingsLanguageModel, nameof(embeddingsLanguageModel));
        ArgumentException.ThrowIfNullOrEmpty(searchString, nameof(searchString));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minResultScore, 1.0f, nameof(minResultScore));
        ArgumentOutOfRangeException.ThrowIfLessThan(minResultScore, 0.0f, nameof(minResultScore));
        ArgumentOutOfRangeException.ThrowIfLessThan<ulong>(maxResults, 1, nameof(maxResults));

        var generated_embeddings = await embeddingsLanguageModel.Generate(searchString, cancellationToken);
        var embedding = new ReadOnlyMemory<float>(generated_embeddings.Embedding);

        var total_documents = await _vector_db_client.CountAsync(_vectorDbCollectionName, cancellationToken: cancellationToken);
        var documents = await _vector_db_client.SearchAsync(_vectorDbCollectionName, embedding, limit: maxResults, scoreThreshold: minResultScore, payloadSelector: true, cancellationToken: cancellationToken);

        // TODO: RAG for ResponseLanguageModel - evaluate building a single prompt with RAG for all document chunks instead of individual prompts.
        var searchResponses = documents.Select(async doc =>
            {
                var documentVectorDbRecord = doc.Payload.ConvertToDocumentVectorDbRecord();

                // TODO: Prompt creation must be be made configurable to fine tune it.
                var prompt = $"Using this content {documentVectorDbRecord.Document} from {documentVectorDbRecord.FileName} .Respond to this prompt: {searchString} without any additional information.";

                var languageResponse = responseLanguageModel is null ? null : await responseLanguageModel.Generate(prompt, cancellationToken);

                return new Common.SearchResponse
                {
                    // TODO: Evaluate changing VectorResponse.Text to contain only fields as chosen (not the entire payload).
                    VectorResponse = new VectorResponse
                    {
                        Id = doc.Id.Uuid,
                        Score = doc.Score,
                        Text = doc.Payload.ToString()
                    },
                    LanguageResponse = languageResponse is null ? null : new LanguageResponse
                    {
                        Model = languageResponse.Model,
                        CreatedAt = languageResponse.CreatedAt,
                        Done = languageResponse.Done,
                        DoneReason = languageResponse.DoneReason,
                        Response = languageResponse.Response
                    }
                };
            });

        _logger.LogTrace($"Returning response(s) from vector db and S/LLM, vector db had total of {total_documents} documents.");

        return searchResponses;
    }

    public async Task<DataQueryResponse?> GetDataAsync(
        Model<VectorEmbeddings> embeddingsLanguageModel,
        Model<LanguageResponse>? responseLanguageModel,
        InfluxDbRepository influxDbRepository,
        string queryString,
        CancellationToken cancellationToken,
        float minResultScore)
    {
        ArgumentNullException.ThrowIfNull(embeddingsLanguageModel, nameof(embeddingsLanguageModel));
        ArgumentNullException.ThrowIfNull(responseLanguageModel, nameof(responseLanguageModel));
        ArgumentNullException.ThrowIfNull(influxDbRepository, nameof(influxDbRepository));
        ArgumentException.ThrowIfNullOrEmpty(queryString, nameof(queryString));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minResultScore, 1.0f, nameof(minResultScore));
        ArgumentOutOfRangeException.ThrowIfLessThan(minResultScore, 0.0f, nameof(minResultScore));

        var generated_embeddings = await embeddingsLanguageModel.Generate(queryString, cancellationToken);
        var embedding = new ReadOnlyMemory<float>(generated_embeddings.Embedding);

        var total_documents = await _vector_db_client.CountAsync(_vectorDbCollectionName, cancellationToken: cancellationToken);

        var documents = await _vector_db_client.SearchAsync(
            _vectorDbCollectionName,
            embedding,
            limit: 1, // Return only first document.
            scoreThreshold: minResultScore,
            payloadSelector: true,
            cancellationToken: cancellationToken);

        if (documents.Count > 0)
        {
            // TODO: Verify the first document with highest score is returned.
            var document = documents[0];
            var dataVectorDbRecord = document.Payload.ConvertToDataVectorDbRecord();

            // TODO: "organization" should come from configuration.
            var data = await influxDbRepository.QueryAsync(dataVectorDbRecord.Query, "organization");

            // TODO: Prompt creation must be be made configurable to fine tune it.
            var data_string = JsonSerializer.Serialize(data.Raw);
            var prompt = $"Using this data {data_string} Respond to this prompt: {queryString} without any additional information.";

            _logger.LogTrace($"Executing language model to generate response for the prompt '{prompt}'.");

            var languageResponse = responseLanguageModel is null ? null : await responseLanguageModel.Generate(prompt, cancellationToken);

            var dataQueryResponse = new DataQueryResponse
            {
                // TODO: evaluate changing VectorResponse.Text to contain only fields as chosen (not the entire payload).
                VectorResponse = new VectorResponse
                {
                    Id = document.Id.Uuid,
                    Score = document.Score,
                    Text = document.Payload.ToString()
                },
                DatabaseQueryResponse = new InfluxDatabaseResponse
                {
                    Raw = data.Raw
                },
                LanguageResponse = languageResponse is null ? null : new LanguageResponse
                {
                    Model = languageResponse.Model,
                    CreatedAt = languageResponse.CreatedAt,
                    Done = languageResponse.Done,
                    DoneReason = languageResponse.DoneReason,
                    Response = languageResponse.Response
                }
            };

            _logger.LogTrace($"Returning response(s) from vector db and S/LLM (optionally), vector db had total of {total_documents} documents.");

            return dataQueryResponse;
        }
        else
        {
            _logger.LogTrace($"No database query was found in the vector db, total queries in vector db are {total_documents}.");
            return null;
        }
    }
}
