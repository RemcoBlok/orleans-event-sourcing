namespace Banking.Events
{
    [Immutable]
    [GenerateSerializer]
    public record PrimaryResidenceChangedEvent(Address Residence);
}