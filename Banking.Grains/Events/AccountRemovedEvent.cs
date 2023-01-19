namespace Banking.Grains.Events
{
    [Immutable]
    [GenerateSerializer]
    public record AccountRemovedEvent(string AccountNumber);
}