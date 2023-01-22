namespace Banking.Persistence.Interfaces
{
    public readonly record struct Result(bool Conflict, string? ETag);
}
