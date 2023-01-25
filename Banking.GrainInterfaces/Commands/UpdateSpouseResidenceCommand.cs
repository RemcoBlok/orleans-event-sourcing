namespace Banking.GrainInterfaces.Commands
{
    [Immutable]
    [GenerateSerializer]
    public record UpdateSpouseResidenceCommand(string CustomerId, Address Residence);
}
