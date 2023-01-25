using Commands = Banking.GrainInterfaces.Commands;

namespace Banking.Grains.Managers
{
    internal static class ConversionExtensions
    {
        public static Events.Person ToEvent(this Commands.Person person)
        {
            return new(
                person.FullName,
                person.FirstName,
                person.LastName,
                person.Residence.ToEvent(),
                person.TaxId,
                person.DateOfBirth);
        }

        public static Events.Address ToEvent(this Commands.Address address)
        {
            return new(
                address.Street,
                address.Street2,
                address.City,
                address.StateOrProvince,
                address.Country,
                address.PostalCode);
        }

        public static Events.Account ToEvent(this Commands.Account account)
        {
            return new(
                account.IsPrimaryAccount,
                account.AccountType,
                account.AccountNumber);
        }
    }
}
