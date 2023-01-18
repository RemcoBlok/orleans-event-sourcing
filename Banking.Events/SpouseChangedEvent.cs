namespace Banking.Events
{
    [Immutable]
    [GenerateSerializer]
    public record SpouseChangedEvent(Person Spouse);
}