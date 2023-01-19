namespace Banking.Persistence.Interfaces
{
    public readonly record struct AppendCategoryEventsResult(bool Conflict, string? ETag);
}
