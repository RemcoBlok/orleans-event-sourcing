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
            if (State.CustomerId != null)
            {
                return;
            }

            RaiseEvent(new CustomerCreatedEvent(
                command.CustomerId,
                ToEvent(command.PrimaryAccountHolder),
                ToEvent(command.MailingAddress)));

            await ConfirmEventsAndStream();
        }

        public async Task UpdatePrimaryAccountHolder(UpdatePrimaryAccountHolderCommand command)
        {
            RaiseEvent(new PrimaryAccountHolderChangedEvent(ToEvent(command.PrimaryAccountHolder)));

            await ConfirmEventsAndStream();
        }

        public async Task UpdatePrimaryResidence(UpdatePrimaryResidenceCommand command)
        {
            if (State.PrimaryAccountHolder == null)
            {
                return;
            }

            RaiseEvent(new PrimaryResidenceChangedEvent(ToEvent(command.Residence)));

            await ConfirmEventsAndStream();
        }

        public async Task UpdateSpouse(UpdateSpouseCommand command)
        {
            RaiseEvent(new SpouseChangedEvent(ToEvent(command.Spouse)));

            await ConfirmEventsAndStream();
        }

        public async Task UpdateSpouseResidence(UpdateSpouseyResidenceCommand command)
        {
            if (State.Spouse == null)
            {
                return;
            }

            RaiseEvent(new SpouseResidenceChangedEvent(ToEvent(command.Residence)));

            await ConfirmEventsAndStream();
        }

        public async Task RemoveSpouse(RemoveSpouseCommand command)
        {
            if (State.Spouse == null)
            {
                return;
            }

            RaiseEvent(new SpouseRemovedEvent());

            await ConfirmEventsAndStream();
        }

        public async Task UpdateMailingAddress(UpdateMailingAddressCommand command)
        {
            RaiseEvent(new MailingAddressChangedEvent(ToEvent(command.MailingAddress)));

            await ConfirmEventsAndStream();
        }

        public async Task AddAccount(AddAccountCommand command)
        {
            if (State.Accounts.Any(account => account.AccountNumber == command.Account.AccountNumber))
            {
                return;
            }

            RaiseEvent(new AccountAddedEvent(ToEvent(command.Account)));

            await ConfirmEventsAndStream();
        }

        public async Task RemoveAccount(RemoveAccountCommand command)
        {
            if (State.Accounts.All(account => account.AccountNumber != command.AccountNumber))
            {
                return;
            }

            RaiseEvent(new AccountRemovedEvent(command.AccountNumber));

            await ConfirmEventsAndStream();
        }

        public async Task PostTransaction(PostTransactionCommand command)
        {
            if (State.Accounts.All(account => account.AccountNumber != command.AccountNumber))
            {
                return;
            }

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

        private static Events.Person ToEvent(GrainInterfaces.Commands.Person person)
        {
            return new(
                person.FullName,
                person.FirstName,
                person.LastName,
                ToEvent(person.Residence),
                person.TaxId,
                person.DateOfBirth);
        }

        private static Events.Address ToEvent(GrainInterfaces.Commands.Address address)
        {
            return new(
                address.Street,
                address.Street2,
                address.City,
                address.StateOrProvince,
                address.Country,
                address.PostalCode);
        }

        private static Events.Account ToEvent(GrainInterfaces.Commands.Account account)
        {
            return new(
                account.IsPrimaryAccount,
                account.AccountType,
                account.AccountNumber);
        }
    }
}