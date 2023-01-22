namespace Banking.GrainInterfaces.Projections
{
    [Immutable]
    [GenerateSerializer]
    public record Account(bool IsPrimaryAccount, string AccountType, string AccountNumber, decimal Balance);
}
