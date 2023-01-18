namespace Banking.GrainInterfaces.Commands
{
    [Immutable]
    [GenerateSerializer]
    public record PostTransactionCommand(string CustomerId, string AccountNumber, decimal Amount);
}
