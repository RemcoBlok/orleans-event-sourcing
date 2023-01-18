namespace Banking.GrainInterfaces.Commands
{
    [Immutable]
    [GenerateSerializer]
    public record Person(string FullName, string FirstName, string LastName, Address Residence, string TaxId, DateOnly DateOfBirth);
}
