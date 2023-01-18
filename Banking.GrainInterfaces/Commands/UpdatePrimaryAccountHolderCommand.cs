namespace Banking.GrainInterfaces.Commands
{
    [Immutable]
    [GenerateSerializer]
    public record UpdatePrimaryAccountHolderCommand(string CustomerId, Person PrimaryAccountHolder);
}
