namespace Banking.Persistence.Interfaces
{
    public interface IEventStorage<TEventBase> where TEventBase : notnull
    {
        Task<IReadOnlyList<TEventBase>> ReadEvents(EventPartitionKey partitionKey);
        Task<bool> AppendEvents(EventPartitionKey partitionKey, IReadOnlyList<TEventBase> events, int expectedVersion);
    }
}
