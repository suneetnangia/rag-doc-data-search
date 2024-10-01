namespace Rag.Common.VectorDb;

/// <summary>
/// Options for the VectorDb.
/// </summary>
public class VectorDbOptions
{
    public const string VectorDb = "VectorDb";

    public required string HostName { get; set; }

    public required int HostPort { get; set; }

    public required string CollectionName { get; set; }
}
