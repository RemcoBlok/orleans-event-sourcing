using Banking.GrainInterfaces.Projectors;
using Banking.Persistence.Interfaces;
using Banking.Persistence.Interfaces.Models;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Streams;

namespace Banking.Grains.Projectors
{
    [ImplicitStreamSubscription(Constants.CategoryEventsStreamNamespace)]
    public class CategoryEventsProjector : JournaledGrain<object>, ICategoryEventsProjector, ICustomStorageInterface<object, object>
    {
        private readonly ICategoryEventsStorage<object> _storage;
        private string? _etag;

        public CategoryEventsProjector(ICategoryEventsStorage<object> storage)
        {
            _storage = storage;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);

            string id = this.GetPrimaryKeyString();
            await this.GetStreamProvider(Constants.StreamProvider)
                .GetStream<object>(Constants.CategoryEventsStreamNamespace, id)
                .SubscribeAsync(OnNextAsync);
        }

        public async Task<KeyValuePair<int, object>> ReadStateFromStorage()
        {
            CategoryEventsPartitionKey partitionKey = GetPartitionKey();
            CheckpointModel checkpoint = await _storage.ReadCheckpoint(partitionKey);
            object state = new();
            _etag = checkpoint.ETag;
            return new(checkpoint.Version, state);
        }

        public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<object> updates, int expectedversion)
        {
            CategoryEventsPartitionKey partitionKey = GetPartitionKey();
            CheckpointModel checkpoint = new(expectedversion, _etag);
            Result result = await _storage.AppendEvents(partitionKey, updates, checkpoint);
            _etag = result.ETag;
            return !result.Conflict;
        }

        public async Task OnNextAsync(IList<SequentialItem<object>> batch)
        {
            RaiseEvents(batch.Select(sequentialItem => sequentialItem.Item));

            await ConfirmEvents();
        }

        private CategoryEventsPartitionKey GetPartitionKey() => new(this.GetGrainId().Key.ToString()!);
    }
}
