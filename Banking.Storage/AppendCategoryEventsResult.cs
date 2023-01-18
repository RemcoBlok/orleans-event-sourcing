namespace Banking.Storage
{
    public readonly record struct AppendCategoryEventsResult(bool Conflict, string? ETag);
}
