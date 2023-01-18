namespace Banking.Events
{
    [Immutable]
    [GenerateSerializer]
    public record PrimaryAccountHolderChangedEvent(Person PrimaryAccountHolder);
}