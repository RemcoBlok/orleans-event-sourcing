using Banking.GrainInterfaces;
using Banking.GrainInterfaces.Commands;
using Banking.Grains.Events;
using Banking.Grains.State;
using Banking.Persistence.Interfaces;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Streams;

namespace Banking.Grains
{
    public class CustomerManager : JournaledGrain<CustomerManagerState>, ICustomerManager, ICustomStorageInterface<CustomerManagerState, object>
    {
        private readonly IEventStorage<object> _storage;
        private IAsyncStream<object>? _categoryEventsStream;
        private IAsyncStream<object>? _customerStream;
        private IAsyncStream<object>? _customersStream;

        public CustomerManager(IEventStorage<object> storage)
        {
            _storage = storage;
        }

        public async Task CreateCustomer(CreateCustomerCommand command)
        {
            RaiseEvent(new CustomerCreatedEvent(
                command.CustomerId,
                GetPerson(command.PrimaryAccountHolder),
                GetAddress(command.MailingAddress)));

            await ConfirmEventsAndStream();
        }

        public async Task UpdatePrimaryAccountHolder(UpdatePrimaryAccountHolderCommand command)
        {
            RaiseEvent(new PrimaryAccountHolderChangedEvent(GetPerson(command.PrimaryAccountHolder)));

            await ConfirmEventsAndStream();
        }

        public async Task UpdatePrimaryResidence(UpdatePrimaryResidenceCommand command)
        {
            RaiseEvent(new PrimaryResidenceChangedEvent(GetAddress(command.Residence)));

            await ConfirmEventsAndStream();
        }

        public async Task UpdateSpouse(UpdateSpouseCommand command)
        {
            RaiseEvent(new SpouseChangedEvent(GetPerson(command.Spouse)));

            await ConfirmEventsAndStream();
        }

        public async Task UpdateSpouseResidence(UpdateSpouseyResidenceCommand command)
        {
            RaiseEvent(new SpouseResidenceChangedEvent(GetAddress(command.Residence)));

            await ConfirmEventsAndStream();
        }

        public async Task RemoveSpouse(RemoveSpouseCommand command)
        {
            RaiseEvent(new SpouseRemovedEvent());

            await ConfirmEventsAndStream();
        }

        public async Task UpdateMailingAddress(UpdateMailingAddressCommand command)
        {
            RaiseEvent(new MailingAddressChangedEvent(GetAddress(command.MailingAddress)));

            await ConfirmEventsAndStream();
        }

        public async Task AddAccount(AddAccountCommand command)
        {
            RaiseEvent(new AccountAddedEvent(GetAccount(command.Account)));

            await ConfirmEventsAndStream();
        }

        public async Task RemoveAccount(RemoveAccountCommand command)
        {
            RaiseEvent(new AccountRemovedEvent(command.AccountNumber));

            await ConfirmEventsAndStream();
        }

        public async Task PostTransaction(PostTransactionCommand command)
        {
            RaiseEvent(new TransactionPostedEvent(command.AccountNumber, command.Amount));

            await ConfirmEventsAndStream();
        }

        public async Task<KeyValuePair<int, CustomerManagerState>> ReadStateFromStorage()
        {
            EventPartitionKey partitionKey = GetPartitionKey();
            IReadOnlyList<object> events = await _storage.ReadEvents(partitionKey);
            CustomerManagerState state = new();
            foreach (object @event in events)
            {
                TransitionState(state, @event);
            }

            return new(events.Count, state);
        }

        public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<object> updates, int expectedversion)
        {
            EventPartitionKey partitionKey = GetPartitionKey();
            return await _storage.AppendEvents(partitionKey, updates, expectedversion);
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            IStreamProvider provider = this.GetStreamProvider(Constants.StreamProvider);
            _categoryEventsStream = provider.GetStream<object>(Constants.CategoryEventsStreamNamespace, nameof(CustomerManager));
            _customerStream = provider.GetStream<object>(Constants.CustomerStreamNamespace, this.GetPrimaryKeyString());
            _customersStream = provider.GetStream<object>(Constants.CustomersStreamNamespace, GrainInterfaces.Constants.AllKey);

            return base.OnActivateAsync(cancellationToken);
        }

        private async Task ConfirmEventsAndStream()
        {
            object[] events = UnconfirmedEvents.ToArray();

            await ConfirmEvents();

            await _categoryEventsStream!.OnNextBatchAsync(events);
            await _customerStream!.OnNextBatchAsync(events);
            await _customersStream!.OnNextBatchAsync(events);
        }

        private EventPartitionKey GetPartitionKey() => new(nameof(CustomerManager), this.GetGrainId().Key.ToString()!);

        private static Events.Person GetPerson(GrainInterfaces.Commands.Person person)
        {
            return new(
                person.FullName,
                person.FirstName,
                person.LastName,
                GetAddress(person.Residence),
                person.TaxId,
                person.DateOfBirth);
        }

        private static Events.Address GetAddress(GrainInterfaces.Commands.Address address)
        {
            return new(
                address.Street,
                address.Street2,
                address.City,
                address.StateOrProvince,
                address.Country,
                address.PostalCode);
        }

        private static Events.Account GetAccount(GrainInterfaces.Commands.Account account)
        {
            return new(
                account.IsPrimaryAccount,
                account.AccountType,
                account.AccountNumber);
        }
    }
}