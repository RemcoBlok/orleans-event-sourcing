using Banking.Events;

namespace Banking.Grains.State
{
    public class CustomerState
    {
        public string? CustomerId { get; private set; } 
        public Person? PrimaryAccountHolder { get; private set; }
        public Person? Spouse { get; private set; }
        public Address? MailingAddress { get; private set; }
        public Account[] Accounts { get; private set; } = Array.Empty<Account>();

        public void Apply(CustomerCreatedEvent @event)
        {
            CustomerId = @event.CustomerId;
            PrimaryAccountHolder = new Person
            {
                FullName = @event.PrimaryAccountHolder.FullName,
                FirstName = @event.PrimaryAccountHolder.FirstName,
                LastName = @event.PrimaryAccountHolder.LastName,
                Residence = new Address
                {
                    Street = @event.PrimaryAccountHolder.Residence.Street,
                    Street2 = @event.PrimaryAccountHolder.Residence.Street2,
                    City = @event.PrimaryAccountHolder.Residence.City,
                    StateOrProvince = @event.PrimaryAccountHolder.Residence.StateOrProvince,
                    Country = @event.PrimaryAccountHolder.Residence.Country,
                    PostalCode = @event.PrimaryAccountHolder.Residence.PostalCode
                },
                TaxId = @event.PrimaryAccountHolder.TaxId,
                DateOfBirth = @event.PrimaryAccountHolder.DateOfBirth
            };
            MailingAddress = new Address
            {
                Street = @event.MailingAddress.Street,
                Street2 = @event.MailingAddress.Street2,
                City = @event.MailingAddress.City,
                StateOrProvince = @event.MailingAddress.StateOrProvince,
                Country = @event.MailingAddress.Country,
                PostalCode = @event.MailingAddress.PostalCode
            };
        }

        public void Apply(PrimaryAccountHolderChangedEvent @event)
        {
            PrimaryAccountHolder = new Person
            {
                FullName = @event.PrimaryAccountHolder.FullName,
                FirstName = @event.PrimaryAccountHolder.FirstName,
                LastName = @event.PrimaryAccountHolder.LastName,
                Residence = new Address
                {
                    Street = @event.PrimaryAccountHolder.Residence.Street,
                    Street2 = @event.PrimaryAccountHolder.Residence.Street2,
                    City = @event.PrimaryAccountHolder.Residence.City,
                    StateOrProvince = @event.PrimaryAccountHolder.Residence.StateOrProvince,
                    Country = @event.PrimaryAccountHolder.Residence.Country,
                    PostalCode = @event.PrimaryAccountHolder.Residence.PostalCode
                },
                TaxId = @event.PrimaryAccountHolder.TaxId,
                DateOfBirth = @event.PrimaryAccountHolder.DateOfBirth
            };
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
            Spouse = new Person
            {
                FullName = @event.Spouse.FullName,
                FirstName = @event.Spouse.FirstName,
                LastName = @event.Spouse.LastName,
                Residence = new Address
                {
                    Street = @event.Spouse.Residence.Street,
                    Street2 = @event.Spouse.Residence.Street2,
                    City = @event.Spouse.Residence.City,
                    StateOrProvince = @event.Spouse.Residence.StateOrProvince,
                    Country = @event.Spouse.Residence.Country,
                    PostalCode = @event.Spouse.Residence.PostalCode
                },
                TaxId = @event.Spouse.TaxId,
                DateOfBirth = @event.Spouse.DateOfBirth
            };
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
            MailingAddress = new Address
            {
                Street = @event.MailingAddress.Street,
                Street2 = @event.MailingAddress.Street2,
                City = @event.MailingAddress.City,
                StateOrProvince = @event.MailingAddress.StateOrProvince,
                Country = @event.MailingAddress.Country,
                PostalCode = @event.MailingAddress.PostalCode
            };
        }

        public void Apply(AccountAddedEvent @event)
        {
            Account account = new()
            {
                IsPrimaryAccount = @event.Account.IsPrimaryAccount,
                AccountType = @event.Account.AccountType,
                AccountNumber = @event.Account.AccountNumber,
                Balance = @event.Account.Balance
            };

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
