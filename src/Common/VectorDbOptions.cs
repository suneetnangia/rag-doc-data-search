namespace Common;

/// <summary>
/// Options for the VectorDb.
/// </summary>
public class VectorDbOptions
{
    public const string VectorDb = "VectorDb";

    // TODO: Move connection string out of this class to adhere to least privilege principle.
    public required string ConnectionString { get; set; }

    public required string CollectionName { get; set; }
}
