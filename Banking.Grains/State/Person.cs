namespace Banking.Grains.State
{
    [Immutable]
    [GenerateSerializer]
    public record Person(string FullName, string FirstName, string LastName, Address Residence, string TaxId, DateOnly DateOfBirth)
    {
        public Person UpdateResidence(Address residence)
        {
            return new(
                FullName,
                FirstName,
                LastName,
                residence,
                TaxId,
                DateOfBirth);
        }

        public Person UpdateResidence(Events.Address residence)
        {
            return UpdateResidence(new Address(
                residence.Street,
                residence.Street2,
                residence.City,
                residence.StateOrProvince,
                residence.Country,
                residence.PostalCode));
        }
    }
}
