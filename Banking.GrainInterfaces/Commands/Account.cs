namespace Banking.GrainInterfaces.Commands
{
    [Immutable]
    [GenerateSerializer]
    public record Account(bool IsPrimaryAccount, string AccountType, string AccountNumber);
}
