﻿using Banking.GrainInterfaces;
using Banking.Grains.State;
using Banking.Persistence.Interfaces;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Streams;

namespace Banking.Grains
{
    [ImplicitStreamSubscription(Constants.CategoryEventsStreamNamespace)]
    public class CategoryEventsProjector : JournaledGrain<CategoryEventsState>, ICategoryEventsProjector, ICustomStorageInterface<CategoryEventsState, object>
    {
        private readonly ICategoryEventsStorage _eventStorage;
        
        public CategoryEventsProjector(ICategoryEventsStorage eventStorage)
        {
            _eventStorage = eventStorage;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {            
            await base.OnActivateAsync(cancellationToken);

            string id = this.GetPrimaryKeyString();
            await this.GetStreamProvider(Constants.StreamProvider)
                .GetStream<object>(Constants.CategoryEventsStreamNamespace, id)
                .SubscribeAsync(OnNextAsync);
        }

        public async Task<KeyValuePair<int, CategoryEventsState>> ReadStateFromStorage()
        {
            CategoryEventsPartitionKey partitionKey = GetPartitionKey();
            CheckpointModel checkpoint = await _eventStorage.ReadCheckpoint(partitionKey);
            CategoryEventsState state = new();
            TransitionState(state, checkpoint.ETag);
            return new(checkpoint.Version, state);
        }

        public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<object> updates, int expectedversion)
        {
            CategoryEventsPartitionKey partitionKey = GetPartitionKey();
            CheckpointModel checkpoint = new(expectedversion, State.ETag);
            AppendCategoryEventsResult result = await _eventStorage.AppendEvents(partitionKey, updates, checkpoint);
            TransitionState(State, result.ETag);
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