namespace Banking.Grains.Events
{
    [Immutable]
    [GenerateSerializer]
    public record AccountAddedEvent(Account Account);
}