namespace Banking.Persistence.Interfaces
{
    public interface ICategoryEventsStorage<TEventBase> where TEventBase : notnull
    {
        Task<CheckpointModel> ReadCheckpoint(CategoryEventsPartitionKey partitionKey);
        Task<Result> AppendEvents(CategoryEventsPartitionKey partitionKey, IReadOnlyList<TEventBase> events, CheckpointModel checkpoint);
    }
}
