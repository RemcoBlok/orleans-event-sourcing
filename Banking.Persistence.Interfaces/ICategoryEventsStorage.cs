namespace Banking.Persistence.Interfaces
{
    public interface ICategoryEventsStorage
    {
        Task<CheckpointModel> ReadCheckpoint(CategoryEventsPartitionKey partitionKey);
        Task<AppendCategoryEventsResult> AppendEvents(CategoryEventsPartitionKey partitionKey, IReadOnlyList<object> events, CheckpointModel checkpoint);
    }
}
