namespace Banking.Grains.Events
{
    [Immutable]
    [GenerateSerializer]
    public record PrimaryAccountHolderChangedEvent(Person PrimaryAccountHolder);
}