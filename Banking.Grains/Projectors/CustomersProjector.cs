using Banking.GrainInterfaces.Hubs;
using Banking.GrainInterfaces.Projections;
using Banking.GrainInterfaces.Projectors;
using Banking.Grains.State;
using Banking.Persistence.Interfaces;
using Banking.Persistence.Interfaces.Models;
using Microsoft.AspNetCore.SignalR.Protocol;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Providers;
using Orleans.Streams;

namespace Banking.Grains.Projectors
{
    [LogConsistencyProvider(ProviderName = Constants.ProjectionStorageName)]
    [ImplicitStreamSubscription(Constants.CustomersStreamNamespace)]
    public class CustomersProjector : JournaledGrain<CustomersProjectorState>, ICustomersProjector, ICustomStorageInterface<CustomersProjectorState, object>
    {
        private readonly SignalR.Orleans.Core.HubContext<NotificationHub> _hubContext;
        private readonly IProjectionStorage<CustomersProjectorState> _storage;
        private string? _etag;

        public CustomersProjector(IProjectionStorage<CustomersProjectorState> storage)//, SignalR.Orleans.Core.HubContext<NotificationHub> hubContext)
        {
            _storage = storage;
            //_hubContext = hubContext;
            _hubContext = GrainFactory.GetHub<NotificationHub>();
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);

            string id = this.GetPrimaryKeyString();
            await this.GetStreamProvider(Constants.StreamProviderName)
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

            InvocationMessage message = new(nameof(INotificationClient.NotifyCustomersProjectionUpdated), Array.Empty<object?>());
            await _hubContext.Client(GrainInterfaces.Constants.NotificationGroup).Send(message);
        }

        public Task<CustomersProjection> GetProjection()
        {
            CustomersProjectorState state = State;
            return Task.FromResult<CustomersProjection>(new
                (
                    state.CustomerCount,
                    state.AccountCount,
                    state.TransactionCount,
                    state.AccountBalanceSum
                ));
        }
    }
}
