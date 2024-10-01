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
            _logger.LogTrace($"Collection {_vectorDbCollectionName} already exists, making use of it.");
        }
        else
        {
            _logger.LogInformation($"Collection {_vectorDbCollectionName} does not exists, creating it.");
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
        string document,
        CancellationToken cancellationToken)
    {
        if (embeddingsLanguageModel is null)
        {
            throw new ArgumentNullException(nameof(embeddingsLanguageModel));
        }

        if (documentId == Guid.Empty)
        {
            throw new ArgumentOutOfRangeException(nameof(documentId));
        }

        if (string.IsNullOrEmpty(document))
        {
            throw new ArgumentNullException(nameof(document));
        }

        var generated_embeddings = await embeddingsLanguageModel.Generate(document, cancellationToken);

        var result = await _vector_db_client.UpsertAsync(
            _vectorDbCollectionName,
            new List<PointStruct>
            {
                new()
                {
                    Id = documentId,
                    Vectors = generated_embeddings.Embedding,
                    Payload =
                    {
                        // TODO: This can be optimized by defining schema/strong type for the payload which can include any key-value pairs.
                        ["document"] = document,
                    }
                }
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
        if (embeddingsLanguageModel is null)
        {
            throw new ArgumentNullException(nameof(embeddingsLanguageModel));
        }

        if (responseLanguageModel is null)
        {
            _logger.LogTrace("Response language model is not provided, using only vector db responses.");
        }

        if (string.IsNullOrEmpty(searchString))
        {
            throw new ArgumentNullException(nameof(searchString));
        }

        if (minResultScore < 0.0f || minResultScore > 1.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(minResultScore));
        }

        if (maxResults < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(minResultScore));
        }

        var generated_embeddings = await embeddingsLanguageModel.Generate(searchString, cancellationToken);
        var embedding = new ReadOnlyMemory<float>(generated_embeddings.Embedding);

        var total_documents = await _vector_db_client.CountAsync(_vectorDbCollectionName, cancellationToken: cancellationToken);
        var documents = await _vector_db_client.SearchAsync(_vectorDbCollectionName, embedding, limit: maxResults, scoreThreshold: minResultScore, payloadSelector: true, cancellationToken: cancellationToken);

        var searchResponses = documents.Select(async doc =>
            {
                // TODO: Prompt creation must be be made configurable to fine tune it.
                var prompt = $"Using this data {doc.Payload.ToString()} Respond to this prompt: {searchString} without any additional information.";

                var languageResponse = responseLanguageModel is null ? null : await responseLanguageModel.Generate(prompt, cancellationToken);

                return new Common.SearchResponse
                {
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
        if (embeddingsLanguageModel is null)
        {
            throw new ArgumentNullException(nameof(embeddingsLanguageModel));
        }

        if (responseLanguageModel is null)
        {
            _logger.LogTrace("Response language model is not provided, using only vector db responses.");
        }

        if (influxDbRepository is null)
        {
            throw new ArgumentNullException(nameof(influxDbRepository));
        }

        if (string.IsNullOrEmpty(queryString))
        {
            throw new ArgumentNullException(nameof(queryString));
        }

        if (minResultScore < 0.0f || minResultScore > 1.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(minResultScore));
        }

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

            // TODO: "document" key should come from the schema/strong type for the payload.
            var string_document = document.Payload["document"]?.ToString();
            var json_document = string_document != null ? JsonDocument.Parse(string_document) : throw new ArgumentNullException(string_document);

            // TODO: "stringValue" key should come from the schema/strong type for the payload.
            var db_query = json_document.RootElement.GetProperty("stringValue").GetString();
            db_query = db_query != null ? db_query : throw new ArgumentNullException(db_query);

            // TODO: "organization" should come from configuration.
            var data = await influxDbRepository.QueryAsync(db_query, "organization");

            // TODO: Prompt creation must be be made configurable to fine tune it.
            var data_string = JsonSerializer.Serialize(data.Raw);
            var prompt = $"Using this data {data_string} Respond to this prompt: {queryString} without any additional information.";

            _logger.LogTrace($"Executing language model to generate response for the prompt '{prompt}'.");

            var languageResponse = responseLanguageModel is null ? null : await responseLanguageModel.Generate(prompt, cancellationToken);

            var dataQueryResponse = new DataQueryResponse
            {
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
