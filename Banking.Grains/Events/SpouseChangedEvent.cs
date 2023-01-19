namespace Banking.Grains.Events
{
    [Immutable]
    [GenerateSerializer]
    public record SpouseChangedEvent(Person Spouse);
}