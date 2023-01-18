namespace Banking.GrainInterfaces.Commands
{
    [Immutable]
    [GenerateSerializer]
    public record UpdatePrimaryResidenceCommand(string CustomerId, Address Residence);
}
