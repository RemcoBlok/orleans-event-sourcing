namespace Banking.GrainInterfaces.Commands
{
    [Immutable]
    [GenerateSerializer]
    public record RemoveAccountCommand(string CustomerId, string AccountNumber);
}
