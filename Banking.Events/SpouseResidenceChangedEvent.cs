namespace Banking.Events
{
    [Immutable]
    [GenerateSerializer]
    public record SpouseResidenceChangedEvent(Address Residence);
}