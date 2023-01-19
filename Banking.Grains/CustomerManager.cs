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
    public class CustomerManager : JournaledGrain<CustomerState>, ICustomerManager, ICustomStorageInterface<CustomerState, object>
    {
        private readonly IEventStorage _eventStorage;
        private IAsyncStream<object>? _stream;

        public CustomerManager(IEventStorage eventStorage)
        {
            _eventStorage = eventStorage;
        }

        public async Task CreateCustomer(CreateCustomerCommand command)
        {
            RaiseEvent(new CustomerCreatedEvent(
                command.CustomerId,
                new Events.Person(
                    command.PrimaryAccountHolder.FullName,
                    command.PrimaryAccountHolder.FirstName,
                    command.PrimaryAccountHolder.LastName,
                    new Events.Address(
                        command.PrimaryAccountHolder.Residence.Street,
                        command.PrimaryAccountHolder.Residence.Street2,
                        command.PrimaryAccountHolder.Residence.City,
                        command.PrimaryAccountHolder.Residence.StateOrProvince,
                        command.PrimaryAccountHolder.Residence.Country,
                        command.PrimaryAccountHolder.Residence.PostalCode),
                    command.PrimaryAccountHolder.TaxId,
                    command.PrimaryAccountHolder.DateOfBirth),
                new Events.Address(
                    command.MailingAddress.Street,
                    command.MailingAddress.Street2,
                    command.MailingAddress.City,
                    command.MailingAddress.StateOrProvince,
                    command.MailingAddress.Country,
                    command.MailingAddress.PostalCode)));

            await ConfirmEventsAndStream();
        }

        public async Task UpdatePrimaryAccountHolder(UpdatePrimaryAccountHolderCommand command)
        {
            RaiseEvent(new PrimaryAccountHolderChangedEvent(
                new Events.Person(
                    command.PrimaryAccountHolder.FullName,
                    command.PrimaryAccountHolder.FirstName,
                    command.PrimaryAccountHolder.LastName,
                    new Events.Address(
                        command.PrimaryAccountHolder.Residence.Street,
                        command.PrimaryAccountHolder.Residence.Street2,
                        command.PrimaryAccountHolder.Residence.City,
                        command.PrimaryAccountHolder.Residence.StateOrProvince,
                        command.PrimaryAccountHolder.Residence.Country,
                        command.PrimaryAccountHolder.Residence.PostalCode),
                    command.PrimaryAccountHolder.TaxId,
                    command.PrimaryAccountHolder.DateOfBirth)));

            await ConfirmEventsAndStream();
        }

        public async Task UpdatePrimaryResidence(UpdatePrimaryResidenceCommand command)
        {
            RaiseEvent(new PrimaryResidenceChangedEvent(
                new Events.Address(
                    command.Residence.Street,
                    command.Residence.Street2,
                    command.Residence.City,
                    command.Residence.StateOrProvince,
                    command.Residence.Country,
                    command.Residence.PostalCode)));

            await ConfirmEventsAndStream();
        }

        public async Task UpdateSpouse(UpdateSpouseCommand command)
        {
            RaiseEvent(new SpouseChangedEvent(
                new Events.Person(
                    command.Spouse.FullName,
                    command.Spouse.FirstName,
                    command.Spouse.LastName,
                    new Events.Address(
                        command.Spouse.Residence.Street,
                        command.Spouse.Residence.Street2,
                        command.Spouse.Residence.City,
                        command.Spouse.Residence.StateOrProvince,
                        command.Spouse.Residence.Country,
                        command.Spouse.Residence.PostalCode),
                    command.Spouse.TaxId,
                    command.Spouse.DateOfBirth)));

            await ConfirmEventsAndStream();
        }

        public async Task UpdateSpouseResidence(UpdateSpouseyResidenceCommand command)
        {
            RaiseEvent(new SpouseResidenceChangedEvent(
                new Events.Address(
                        command.Residence.Street,
                        command.Residence.Street2,
                        command.Residence.City,
                        command.Residence.StateOrProvince,
                        command.Residence.Country,
                        command.Residence.PostalCode)));

            await ConfirmEventsAndStream();
        }

        public async Task RemoveSpouse(RemoveSpouseCommand command)
        {
            RaiseEvent(new SpouseRemovedEvent());

            await ConfirmEventsAndStream();
        }

        public async Task UpdateMailingAddress(UpdateMailingAddressCommand command)
        {
            RaiseEvent(new MailingAddressChangedEvent(
                new Events.Address(
                    command.MailingAddress.Street,
                    command.MailingAddress.Street2,
                    command.MailingAddress.City,
                    command.MailingAddress.StateOrProvince,
                    command.MailingAddress.Country,
                    command.MailingAddress.PostalCode)));

            await ConfirmEventsAndStream();
        }

        public async Task AddAccount(AddAccountCommand command)
        {
            RaiseEvent(new AccountAddedEvent(
                new Events.Account(
                    command.Account.IsPrimaryAccount,
                    command.Account.AccountType,
                    command.Account.AccountNumber,
                    command.Account.Balance)));

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

        public async Task<KeyValuePair<int, CustomerState>> ReadStateFromStorage()
        {
            EventPartitionKey partitionKey = GetPartitionKey();
            IReadOnlyList<object> events = await _eventStorage.ReadEvents(partitionKey);
            CustomerState state = new();
            foreach (object @event in events)
            {
                TransitionState(state, @event);
            }

            return new(events.Count, state);
        }

        public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<object> updates, int expectedversion)
        {
            EventPartitionKey partitionKey = GetPartitionKey();
            return await _eventStorage.AppendEvents(partitionKey, updates, expectedversion);
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            string id = GetType().Name;
            _stream = this.GetStreamProvider(Constants.StreamProvider)
                .GetStream<object>(Constants.CategoryEventsStreamNamespace, id);

            return base.OnActivateAsync(cancellationToken);
        }

        private EventPartitionKey GetPartitionKey() => new(GetType().Name, this.GetGrainId().Key.ToString()!);

        private async Task ConfirmEventsAndStream()
        {
            List<object> events = UnconfirmedEvents.ToList();
            
            await ConfirmEvents();

            await _stream!.OnNextBatchAsync(events);
        }
    }
}