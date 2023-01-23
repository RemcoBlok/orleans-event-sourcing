namespace Banking.Persistence.Interfaces.Models
{
    public readonly record struct Result(bool Conflict, string? ETag);
}
