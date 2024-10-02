namespace Rag.Common.VectorDb;

using Google.Protobuf.Collections;
using Qdrant.Client.Grpc;

internal static class QdrantExtensionMethods
{
    public static DocumentVectorDbRecord ConvertToDocumentVectorDbRecord(this MapField<string, Value> payload)
    {
        return new DocumentVectorDbRecord
        {
            Document = payload.ContainsKey("document") ? payload["document"].StringValue : throw new InvalidOperationException("document key is required from vector db."),
            Tags = payload.ContainsKey("tags") ? payload["tags"].StringValue : null,
            FileName = payload.ContainsKey("filename") ? payload["filename"].StringValue : null,
            Page = payload.ContainsKey("page") ? payload["page"].StringValue : null
        };
    }

    public static DataVectorDbRecord ConvertToDataVectorDbRecord(this MapField<string, Value> payload)
    {
        return new DataVectorDbRecord
        {
            Document = payload.ContainsKey("document") ? payload["document"].StringValue : throw new InvalidOperationException("document key is required from vector db."),
            Tags = payload.ContainsKey("tags") ? payload["tags"].StringValue : null,
            Query = payload.ContainsKey("query") ? payload["query"].StringValue : throw new InvalidOperationException("query key is required from vector db.")
        };
    }

    public static MapField<string, Value> ConvertToMapField(this DocumentVectorDbRecord documentVectorDbRecord)
    {
        return new MapField<string, Value>
        {
            { "document", new Value { StringValue = documentVectorDbRecord.Document } },
            { "tags", new Value { StringValue = documentVectorDbRecord.Tags } },
            { "filename", new Value { StringValue = documentVectorDbRecord.FileName } },
            { "page", new Value { StringValue = documentVectorDbRecord.Page } }
        };
    }

    public static MapField<string, Value> ConvertToMapField(this DataVectorDbRecord dataVectorDbRecord)
    {
        return new MapField<string, Value>
        {
            { "document", new Value { StringValue = dataVectorDbRecord.Document } },
            { "tags", new Value { StringValue = dataVectorDbRecord.Tags } },
            { "query", new Value { StringValue = dataVectorDbRecord.Query } }
        };
    }
}
