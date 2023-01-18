namespace Banking.Storage
{
    public interface IEventStorage
    {
        Task<IReadOnlyList<object>> ReadEvents(EventPartitionKey partitionKey);
        Task<bool> AppendEvents(EventPartitionKey partitionKey, IReadOnlyList<object> events, int expectedVersion);
    }
}
