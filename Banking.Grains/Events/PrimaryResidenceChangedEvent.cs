namespace Banking.Grains.Events
{
    [Immutable]
    [GenerateSerializer]
    public record PrimaryResidenceChangedEvent(Address Residence);
}