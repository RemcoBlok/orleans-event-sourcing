using Banking.Persistence.Interfaces;
using Banking.Persistence.Interfaces.Models;

namespace Banking.Persistence.Redis
{
    public class EventStorage<TEventBase> : IEventStorage<TEventBase> where TEventBase : notnull
    {
        public Task<bool> AppendEvents(EventPartitionKey partitionKey, IReadOnlyList<TEventBase> events, int expectedVersion)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<TEventBase>> ReadEvents(EventPartitionKey partitionKey)
        {
            throw new NotImplementedException();
        }
    }
}
