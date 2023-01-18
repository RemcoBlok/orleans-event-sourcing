namespace Banking.GrainInterfaces.Commands
{
    [Immutable]
    [GenerateSerializer]
    public record AddAccountCommand(string CustomerId, Account Account);
}
