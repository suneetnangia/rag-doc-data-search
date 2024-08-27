namespace Common;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Reflection;
using Google.Protobuf.Collections;

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

    /// <summary>
    /// Initialize the vector database.
    /// </summary>
    /// <returns></returns>
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
            await _vector_db_client.CreateCollectionAsync(collectionName: _vectorDbCollectionName, vectorsConfig: new VectorParams
            {
                // TODO: Vector dimension and size can be read from configuration or inferred from the embedding model.
                Size = 1024,
                Distance = Distance.Cosine
            }, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Add document into the vector database.
    /// </summary>
    public async Task AddDocumentAsync(
        LanguageModel<VectorEmbeddings> embeddingsLanguageModel,
        Guid documentId,
        VectorDbPayload payload,
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

        if (payload is null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        var generated_embeddings = await embeddingsLanguageModel.Generate(payload.Document, cancellationToken);
        var pointStruct = new PointStruct
        {
           
            Id = documentId,
            Vectors = generated_embeddings.Embedding,
                
        };
        foreach (PropertyInfo property in payload.GetType().GetProperties())
        {
            if (property.GetValue(payload) == null)
            {
                continue;
            }
            // TODO: currently we are assuming that all properties are strings, can be improved to handle other types
            pointStruct.Payload[property.Name.ToLowerInvariant()] = property.GetValue(payload)?.ToString() ?? string.Empty;
        }
        var pointStructList = new List<PointStruct> { pointStruct };
        var result = await _vector_db_client.UpsertAsync(_vectorDbCollectionName, pointStructList, cancellationToken: cancellationToken);

        _logger.LogTrace($"Inserted embeddings in vector db, document : {result}");
    }

    public async Task<IEnumerable<Task<SearchResponse?>>> GetDocumentsAsync(
        LanguageModel<VectorEmbeddings> embeddingsLanguageModel,
        LanguageModel<LanguageResponse>? responseLanguageModel,
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
        // TODO: evaluate using search on payload schema names and not only on embeddings
        var documents = await _vector_db_client.SearchAsync(_vectorDbCollectionName, embedding, limit: maxResults, scoreThreshold: minResultScore, payloadSelector: true, cancellationToken: cancellationToken);

        // TODO: this can be changed to have LLM prompt with RAG from all documents instead one by one, to have a consolidated answer
        var searchResponses = documents.Select(async doc =>
            {
                QdrantDocumentVectorPayload queryDocument = DeserializePayload<QdrantDocumentVectorPayload>(doc.Payload);
                
                // TODO: Prompt creation must be be made configurable to fine tune it.
                var prompt = $"Using this data {queryDocument.Document} Respond to this prompt: {searchString} without any additional information.";
                _logger.LogTrace($"Executing language model to generate response for the prompt '{prompt}'.");

                var languageResponse = responseLanguageModel is null ? null : await responseLanguageModel.Generate(prompt, cancellationToken);

                return new SearchResponse
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
            }
        );

        _logger.LogTrace($"Returning response(s) from vector db and S/LLM, vector db had total of {total_documents} documents.");

        return searchResponses;
    }

    public async Task<DataQueryResponse?> GetDataAsync(
        LanguageModel<VectorEmbeddings> embeddingsLanguageModel,
        LanguageModel<LanguageResponse>? responseLanguageModel,
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
            QdrantQueryDataVectorPayload queryDocument = DeserializePayload<QdrantQueryDataVectorPayload>(document.Payload);
            _logger.LogTrace($"The fields are filled in '{queryDocument.Document}' / and query {queryDocument.Query}.");

            // TODO: "stringValue" key should come from the schema/strong type for the payload.
            var db_query = queryDocument.Query;

            // TODO: "organization" should come from configuration.
            var data = await influxDbRepository.QueryAsync(db_query, "organization");

            // TODO: Prompt creation must be be made configurable to fine tune it.                        
            var data_string =  JsonSerializer.Serialize(data.Raw);
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

    private T DeserializePayload<T>(MapField<string, Value> payload)
    {
        var result = Activator.CreateInstance<T>();
        var properties = typeof(T).GetProperties();
       
       foreach (var property in properties)
        {
            if (payload.TryGetValue(property.Name.ToLowerInvariant(), out var value))
            {
                if (property.PropertyType == typeof(string) && value.KindCase == Value.KindOneofCase.StringValue)
                {
                    property.SetValue(result, value.StringValue);
                }
                else if (property.PropertyType == typeof(double) && value.KindCase == Value.KindOneofCase.DoubleValue)
                {
                    property.SetValue(result, value.DoubleValue);
                }
                // Add more type checks and conversions as needed
            }
        }

        return result;
    }

}
