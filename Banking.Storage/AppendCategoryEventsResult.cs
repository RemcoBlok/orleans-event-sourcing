namespace Banking.Storage
{
    public readonly record struct AppendCategoryEventsResult(bool Success, string? ETag);
}
