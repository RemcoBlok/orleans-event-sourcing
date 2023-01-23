namespace Banking.Persistence.Interfaces.Models
{
    public readonly record struct CheckpointModel(int Version, string? ETag);
}
