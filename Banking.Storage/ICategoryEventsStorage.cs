namespace Banking.Storage
{
    public interface ICategoryEventsStorage
    {
        Task<CategoryEventsModel> ReadEvents(CategoryEventsPartitionKey partitionKey);
        Task<AppendCategoryEventsResult> AppendEvents(CategoryEventsPartitionKey partitionKey, IReadOnlyList<object> events, CategoryEventsModel expected);
    }
}
