namespace Banking.GrainInterfaces.Commands
{
    [Immutable]
    [GenerateSerializer]
    public record UpdateSpouseCommand(string CustomerId, Person Spouse);
}
