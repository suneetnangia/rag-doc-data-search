namespace Rag.Common.VectorDb;

public class VectorDbRecord
{
    public required string Document { get; set; }

    public string? Tags { get; set; }
}
