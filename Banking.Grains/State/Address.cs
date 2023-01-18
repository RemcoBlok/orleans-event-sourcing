namespace Banking.Grains.State
{
    public class Address
    {
        public required string Street { get; init; }
        public required string Street2 { get; init; }
        public required string City { get; init; }
        public required string StateOrProvince { get; init; }
        public required string Country { get; init; }
        public required string PostalCode { get; init; }
    }
}
