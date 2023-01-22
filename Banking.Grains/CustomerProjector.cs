using Banking.GrainInterfaces;
using Banking.GrainInterfaces.Projections;
using Banking.Grains.State;
using Banking.Persistence.Interfaces;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Streams;
using System.Diagnostics.CodeAnalysis;

namespace Banking.Grains
{
    [ImplicitStreamSubscription(Constants.CustomerStreamNamespace)]
    public class CustomerProjector : JournaledGrain<CustomerProjectorState>, ICustomerProjector, ICustomStorageInterface<CustomerProjectorState, object>
    {
        private readonly IProjectionStorage<CustomerProjectorState> _storage;
        private string? _etag;

        public CustomerProjector(IProjectionStorage<CustomerProjectorState> storage)
        {
            _storage = storage;
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
        }

        public Task<CustomerProjection> GetProjection()
        {
            return Task.FromResult<CustomerProjection>(new(
                State.CustomerId,
                ToProjection(State.PrimaryAccountHolder),
                ToProjection(State.Spouse),
                ToProjection(State.MailingAddress),
                State.Accounts.Select(ToProjection).ToArray()));
        }

        [return: NotNullIfNotNull(nameof(person))]
        private static GrainInterfaces.Projections.Person? ToProjection(State.Person? person)
        {
            if (person == null)
            {
                return null;
            }

            return new(
                person.FullName,
                person.FirstName,
                person.LastName,
                ToProjection(person.Residence),
                person.TaxId,
                person.DateOfBirth);
        }

        [return: NotNullIfNotNull(nameof(address))]
        private static GrainInterfaces.Projections.Address? ToProjection(State.Address? address)
        {
            if (address == null)
            {
                return null;
            }

            return new(
                address.Street,
                address.Street2,
                address.City,
                address.StateOrProvince,
                address.Country,
                address.PostalCode);
        }

        private static GrainInterfaces.Projections.Account ToProjection(State.Account account)
        {
            return new(
                account.IsPrimaryAccount,
                account.AccountType,
                account.AccountNumber,
                account.Balance);
        }
    }
}
