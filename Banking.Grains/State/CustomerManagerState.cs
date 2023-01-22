using Banking.Grains.Events;

namespace Banking.Grains.State
{
    public class CustomerManagerState
    {
        public string? CustomerId { get; private set; } 
        public Person? PrimaryAccountHolder { get; private set; }
        public Person? Spouse { get; private set; }
        public Address? MailingAddress { get; private set; }
        public Account[] Accounts { get; private set; } = Array.Empty<Account>();

        public void Apply(CustomerCreatedEvent @event)
        {
            CustomerId = @event.CustomerId;
            PrimaryAccountHolder = GetPerson(@event.PrimaryAccountHolder);
            MailingAddress = GetAddress(@event.MailingAddress);
        }

        public void Apply(PrimaryAccountHolderChangedEvent @event)
        {
            PrimaryAccountHolder = GetPerson(@event.PrimaryAccountHolder);
        }

        public void Apply(PrimaryResidenceChangedEvent @event)
        {
            if (PrimaryAccountHolder == null)
            {
                return;
            }

            PrimaryAccountHolder = PrimaryAccountHolder.UpdateResidence(@event.Residence);
        }

        public void Apply(SpouseChangedEvent @event)
        {
            Spouse = GetPerson(@event.Spouse);
        }

        public void Apply(SpouseResidenceChangedEvent @event)
        {
            if (Spouse == null)
            {
                return;
            }

            Spouse = Spouse.UpdateResidence(@event.Residence);
        }

        public void Apply(SpouseRemovedEvent _)
        {
            Spouse = null;
        }

        public void Apply(MailingAddressChangedEvent @event)
        {
            MailingAddress = GetAddress(@event.MailingAddress);
        }

        public void Apply(AccountAddedEvent @event)
        {
            Account account = GetAccount(@event.Account);

            Accounts = Accounts.Append(account).ToArray();
        }

        public void Apply(AccountRemovedEvent @event)
        {
            Accounts = Accounts.Where(account => account.AccountNumber != @event.AccountNumber).ToArray();
        }

        public void Apply(TransactionPostedEvent @event)
        {
            Account account = Accounts.First(a => a.AccountNumber == @event.AccountNumber).UpdateBalance(@event.Amount);

            Accounts = Accounts.Where(account => account.AccountNumber != @event.AccountNumber).Append(account).ToArray();
        }

        private static Person GetPerson(Events.Person person)
        {
            return new(
                person.FullName,
                person.FirstName,
                person.LastName,
                GetAddress(person.Residence),
                person.TaxId,
                person.DateOfBirth);
        }

        private static Address GetAddress(Events.Address address)
        {
            return new(
                address.Street,
                address.Street2,
                address.City,
                address.StateOrProvince,
                address.Country,
                address.PostalCode);
        }

        private static Account GetAccount(Events.Account account)
        {
            return new(
                account.IsPrimaryAccount,
                account.AccountType,
                account.AccountNumber,
                0m);
        }
    }
}
