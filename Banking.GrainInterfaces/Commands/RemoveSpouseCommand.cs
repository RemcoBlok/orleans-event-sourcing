namespace Banking.GrainInterfaces.Commands
{
    [Immutable]
    [GenerateSerializer]
    public record RemoveSpouseCommand(string CustomerId);
}
