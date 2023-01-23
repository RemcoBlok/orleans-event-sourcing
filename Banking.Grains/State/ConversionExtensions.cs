namespace Banking.Grains.State
{
    internal static class ConversionExtensions
    {
        public static Person ToState(this Events.Person person)
        {
            return new(
                person.FullName,
                person.FirstName,
                person.LastName,
                person.Residence.ToState(),
                person.TaxId,
                person.DateOfBirth);
        }

        public static Address ToState(this Events.Address address)
        {
            return new(
                address.Street,
                address.Street2,
                address.City,
                address.StateOrProvince,
                address.Country,
                address.PostalCode);
        }

        public static Account ToState(this Events.Account account)
        {
            return new(
                account.IsPrimaryAccount,
                account.AccountType,
                account.AccountNumber,
                0m);
        }
    }
}
