﻿using Banking.Grains.Events;
using System.Text.Json.Serialization;

namespace Banking.Grains.State
{
    [GenerateSerializer]
    public class CustomerProjectorState
    {
        [JsonInclude]
        [Id(0)]
        public string? CustomerId { get; private set; }

        [JsonInclude]
        [Id(1)]
        public Person? PrimaryAccountHolder { get; private set; }

        [JsonInclude]
        [Id(2)]
        public Person? Spouse { get; private set; }

        [JsonInclude]
        [Id(3)]
        public Address? MailingAddress { get; private set; }

        [JsonInclude]
        [Id(4)]
        public Account[] Accounts { get; private set; } = Array.Empty<Account>();
        
        public void Apply(CustomerCreatedEvent @event)
        {
            CustomerId = @event.CustomerId;
            PrimaryAccountHolder = ToState(@event.PrimaryAccountHolder);
            MailingAddress = ToState(@event.MailingAddress);
        }

        public void Apply(PrimaryAccountHolderChangedEvent @event)
        {
            PrimaryAccountHolder = ToState(@event.PrimaryAccountHolder);
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
            Spouse = ToState(@event.Spouse);
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
            MailingAddress = ToState(@event.MailingAddress);
        }

        public void Apply(AccountAddedEvent @event)
        {
            Account account = ToState(@event.Account);

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

        private static Person ToState(Events.Person person)
        {
            return new(
                person.FullName,
                person.FirstName,
                person.LastName,
                ToState(person.Residence),
                person.TaxId,
                person.DateOfBirth);
        }

        private static Address ToState(this Events.Address address)
        {
            return new(
                address.Street,
                address.Street2,
                address.City,
                address.StateOrProvince,
                address.Country,
                address.PostalCode);
        }

        private static Account ToState(Events.Account account)
        {
            return new(
                account.IsPrimaryAccount,
                account.AccountType,
                account.AccountNumber,
                0m);
        }
    }
}