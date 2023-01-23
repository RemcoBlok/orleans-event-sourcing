using System.Diagnostics.CodeAnalysis;

namespace Banking.Grains
{
    internal static class ConversionExtensions
    {
        public static Events.Person ToEvent(this GrainInterfaces.Commands.Person person)
        {
            return new(
                person.FullName,
                person.FirstName,
                person.LastName,
                person.Residence.ToEvent(),
                person.TaxId,
                person.DateOfBirth);
        }

        public static Events.Address ToEvent(this GrainInterfaces.Commands.Address address)
        {
            return new(
                address.Street,
                address.Street2,
                address.City,
                address.StateOrProvince,
                address.Country,
                address.PostalCode);
        }

        public static Events.Account ToEvent(this GrainInterfaces.Commands.Account account)
        {
            return new(
                account.IsPrimaryAccount,
                account.AccountType,
                account.AccountNumber);
        }

        [return: NotNullIfNotNull(nameof(person))]
        public static GrainInterfaces.Projections.Person? ToProjection(this State.Person? person)
        {
            if (person == null)
            {
                return null;
            }

            return new(
                person.FullName,
                person.FirstName,
                person.LastName,
                person.Residence.ToProjection(),
                person.TaxId,
                person.DateOfBirth);
        }

        [return: NotNullIfNotNull(nameof(address))]
        public static GrainInterfaces.Projections.Address? ToProjection(this State.Address? address)
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

        public static GrainInterfaces.Projections.Account ToProjection(this State.Account account)
        {
            return new(
                account.IsPrimaryAccount,
                account.AccountType,
                account.AccountNumber,
                account.Balance);
        }
    }
}
