namespace Banking.Grains.State
{
    public class Person
    {
        public required string FullName { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required Address Residence { get; init; }
        public required string TaxId { get; init; }
        public required DateOnly DateOfBirth { get; init; }

        public Person UpdateResidence(Address residence)
        {
            return new Person
            {
                FullName = FullName,
                FirstName = FirstName,
                LastName = LastName,
                Residence = residence,
                TaxId = TaxId,
                DateOfBirth = DateOfBirth,
            };
        }

        public Person UpdateResidence(Events.Address residence)
        {
            return UpdateResidence(new Address
            {
                Street = residence.Street,
                Street2 = residence.Street2,
                City = residence.City,
                StateOrProvince = residence.StateOrProvince,
                Country = residence.Country,
                PostalCode = residence.PostalCode
            });
        }
    }
}
