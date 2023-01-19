namespace Banking.Grains.Events
{
    [Immutable]
    [GenerateSerializer]
    public record Address(string Street, string Street2, string City, string StateOrProvince, string Country, string PostalCode);
}
