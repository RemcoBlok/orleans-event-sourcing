using Banking.Persistence.Interfaces;
using Banking.Persistence.Interfaces.Models;

namespace Banking.Persistence.Redis
{
    public class CategoryEventsStorage<TEventBase> : ICategoryEventsStorage<TEventBase> where TEventBase : notnull
    {
        public Task<Result> AppendEvents(CategoryEventsPartitionKey partitionKey, IReadOnlyList<TEventBase> events, CheckpointModel checkpoint)
        {
            throw new NotImplementedException();
        }

        public Task<CheckpointModel> ReadCheckpoint(CategoryEventsPartitionKey partitionKey)
        {
            throw new NotImplementedException();
        }
    }
}
