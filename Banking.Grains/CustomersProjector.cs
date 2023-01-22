using Banking.GrainInterfaces;
using Banking.GrainInterfaces.Projections;
using Banking.Grains.State;
using Banking.Persistence.Interfaces;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Streams;

namespace Banking.Grains
{
    [ImplicitStreamSubscription(Constants.CustomersStreamNamespace)]
    public class CustomersProjector : JournaledGrain<CustomersProjectorState>, ICustomersProjector, ICustomStorageInterface<CustomersProjectorState, object>
    {
        private readonly IProjectionStorage<CustomersProjectorState> _storage;
        private string? _etag;

        public CustomersProjector(IProjectionStorage<CustomersProjectorState> storage)
        {
            _storage = storage;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);

            string id = this.GetPrimaryKeyString();
            await this.GetStreamProvider(Constants.StreamProvider)
                .GetStream<object>(Constants.CustomersStreamNamespace, id)
                .SubscribeAsync(OnNextAsync);
        }

        public async Task<KeyValuePair<int, CustomersProjectorState>> ReadStateFromStorage()
        {
            ProjectionModel<CustomersProjectorState> projection = await _storage.ReadState(nameof(CustomersProjector), this.GetPrimaryKeyString());
            _etag = projection.ETag;
            return new(projection.Metadata.Version, projection.Data);
        }

        public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<object> updates, int expectedversion)
        {
            ProjectionModel<CustomersProjectorState>? projection = new()
            {
                Data = TentativeState,
                Metadata = new()
                {
                    Version = expectedversion
                },
                ETag = _etag
            };

            Result result = await _storage.SaveState(nameof(CustomersProjector), this.GetPrimaryKeyString(), projection);
            _etag = result.ETag;
            return !result.Conflict;
        }

        public async Task OnNextAsync(IList<SequentialItem<object>> batch)
        {
            RaiseEvents(batch.Select(sequentialItem => sequentialItem.Item));

            await ConfirmEvents();
        }

        public Task<CustomersProjection> GetProjection()
        {
            return Task.FromResult<CustomersProjection>(new
                (
                    State.CustomerCount,
                    State.AccountCount,
                    State.TransactionCount,
                    State.AccountBalanceSum
                ));
        }
    }
}
