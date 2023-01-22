namespace Banking.GrainInterfaces.Projections
{
    [Immutable]
    [GenerateSerializer]
    public record CustomerProjection(string? CustomerId, Person? PrimaryAccountHolder, Person? Spouse, Address? MailingAddress, Account[] Accounts);
}
