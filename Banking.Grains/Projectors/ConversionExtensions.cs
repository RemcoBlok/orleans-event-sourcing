using System.Diagnostics.CodeAnalysis;
using Projections = Banking.GrainInterfaces.Projections;

namespace Banking.Grains.Projectors
{
    internal static class ConversionExtensions
    {
        [return: NotNullIfNotNull(nameof(person))]
        public static Projections.Person? ToProjection(this State.Person? person)
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
        public static Projections.Address? ToProjection(this State.Address? address)
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

        public static Projections.Account ToProjection(this State.Account account)
        {
            return new(
                account.IsPrimaryAccount,
                account.AccountType,
                account.AccountNumber,
                account.Balance);
        }
    }
}
