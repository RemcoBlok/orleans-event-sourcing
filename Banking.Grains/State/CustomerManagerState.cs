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
            PrimaryAccountHolder = @event.PrimaryAccountHolder.ToState();
            MailingAddress = @event.MailingAddress.ToState();
        }

        public void Apply(PrimaryAccountHolderChangedEvent @event)
        {
            PrimaryAccountHolder = @event.PrimaryAccountHolder.ToState();
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
            Spouse = @event.Spouse.ToState();
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
            MailingAddress = @event.MailingAddress.ToState();
        }

        public void Apply(AccountAddedEvent @event)
        {
            Account account = @event.Account.ToState();

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
    }
}
