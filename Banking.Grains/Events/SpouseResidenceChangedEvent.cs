namespace Banking.Grains.Events
{
    [Immutable]
    [GenerateSerializer]
    public record SpouseResidenceChangedEvent(Address Residence);
}