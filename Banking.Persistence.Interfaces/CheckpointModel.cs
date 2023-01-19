namespace Banking.Persistence.Interfaces
{
    public readonly record struct CheckpointModel(int Version, string? ETag);
}
