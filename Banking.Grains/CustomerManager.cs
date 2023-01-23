using Banking.GrainInterfaces;
using Banking.GrainInterfaces.Commands;
using Banking.Grains.Events;
using Banking.Grains.State;
using Banking.Persistence.Interfaces;
using Banking.Persistence.Interfaces.Models;
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
                command.PrimaryAccountHolder.ToEvent(),
                command.MailingAddress.ToEvent()));

            await ConfirmEventsAndStream();
        }

        public async Task UpdatePrimaryAccountHolder(UpdatePrimaryAccountHolderCommand command)
        {
            RaiseEvent(new PrimaryAccountHolderChangedEvent(command.PrimaryAccountHolder.ToEvent()));

            await ConfirmEventsAndStream();
        }

        public async Task UpdatePrimaryResidence(UpdatePrimaryResidenceCommand command)
        {
            if (State.PrimaryAccountHolder == null)
            {
                return;
            }

            RaiseEvent(new PrimaryResidenceChangedEvent(command.Residence.ToEvent()));

            await ConfirmEventsAndStream();
        }

        public async Task UpdateSpouse(UpdateSpouseCommand command)
        {
            RaiseEvent(new SpouseChangedEvent(command.Spouse.ToEvent()));

            await ConfirmEventsAndStream();
        }

        public async Task UpdateSpouseResidence(UpdateSpouseyResidenceCommand command)
        {
            if (State.Spouse == null)
            {
                return;
            }

            RaiseEvent(new SpouseResidenceChangedEvent(command.Residence.ToEvent()));

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
            RaiseEvent(new MailingAddressChangedEvent(command.MailingAddress.ToEvent()));

            await ConfirmEventsAndStream();
        }

        public async Task AddAccount(AddAccountCommand command)
        {
            if (State.Accounts.Any(account => account.AccountNumber == command.Account.AccountNumber))
            {
                return;
            }

            RaiseEvent(new AccountAddedEvent(command.Account.ToEvent()));

            await ConfirmEventsAndStream();
        }

        public async Task RemoveAccount(RemoveAccountCommand command)
        {
            if (State.Accounts.All(account => account.AccountNumber != command.AccountNumber))
            {
                return;
            }

            State.Account account = State.Accounts.First(a => a.AccountNumber == command.AccountNumber);
            if (account.Balance != 0m)
            {
                throw new InvalidOperationException("Balance not zero");
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

            State.Account account = State.Accounts.First(a => a.AccountNumber == command.AccountNumber);
            if (account.Balance + command.Amount < 0m)
            {
                throw new InvalidOperationException("Transaction results in negative balance");
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
    }
}