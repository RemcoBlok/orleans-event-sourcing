using Banking.GrainInterfaces.Hubs;
using Banking.GrainInterfaces.Projections;
using Banking.GrainInterfaces.Projectors;
using Banking.Grains.State;
using Banking.Persistence.Interfaces;
using Banking.Persistence.Interfaces.Models;
using Microsoft.AspNetCore.SignalR.Protocol;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Streams;

namespace Banking.Grains.Projectors
{
    [ImplicitStreamSubscription(Constants.CustomerStreamNamespace)]
    public class CustomerProjector : JournaledGrain<CustomerProjectorState>, ICustomerProjector, ICustomStorageInterface<CustomerProjectorState, object>
    {
        private readonly SignalR.Orleans.Core.HubContext<NotificationHub> _hubContext;
        private readonly IProjectionStorage<CustomerProjectorState> _storage;
        private string? _etag;

        public CustomerProjector(IProjectionStorage<CustomerProjectorState> storage, SignalR.Orleans.Core.HubContext<NotificationHub> hubContext)
        {
            _storage = storage;
            _hubContext = hubContext;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);

            string id = this.GetPrimaryKeyString();
            await this.GetStreamProvider(Constants.StreamProvider)
                .GetStream<object>(Constants.CustomerStreamNamespace, id)
                .SubscribeAsync(OnNextAsync);
        }

        public async Task<KeyValuePair<int, CustomerProjectorState>> ReadStateFromStorage()
        {
            ProjectionModel<CustomerProjectorState> projection = await _storage.ReadState(nameof(CustomerProjector), this.GetPrimaryKeyString());
            _etag = projection.ETag;
            return new(projection.Metadata.Version, projection.Data);
        }

        public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<object> updates, int expectedversion)
        {
            ProjectionModel<CustomerProjectorState>? projection = new()
            {
                Data = TentativeState,
                Metadata = new()
                {
                    Version = expectedversion
                },
                ETag = _etag
            };

            Result result = await _storage.SaveState(nameof(CustomerProjector), this.GetPrimaryKeyString(), projection);
            _etag = result.ETag;
            return !result.Conflict;
        }

        public async Task OnNextAsync(IList<SequentialItem<object>> batch)
        {
            RaiseEvents(batch.Select(sequentialItem => sequentialItem.Item));

            await ConfirmEvents();

            InvocationMessage message = new(nameof(INotificationClient.NotifyCustomerProjectionUpdated), new object?[] { this.GetPrimaryKeyString() });
            await _hubContext.Group(GrainInterfaces.Constants.NotificationGroup).Send(message);
        }

        public Task<CustomerProjection> GetProjection()
        {
            CustomerProjectorState state = State;
            return Task.FromResult<CustomerProjection>(new(
                state.CustomerId,
                state.PrimaryAccountHolder.ToProjection(),
                state.Spouse.ToProjection(),
                state.MailingAddress.ToProjection(),
                state.Accounts.Select(ConversionExtensions.ToProjection).ToArray()));
        }
    }
}
