namespace Banking.GrainInterfaces.Commands
{
    [Immutable]
    [GenerateSerializer]
    public record UpdateSpouseyResidenceCommand(string CustomerId, Address Residence);
}
