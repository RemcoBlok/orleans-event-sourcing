namespace Banking.Events
{
    [Immutable]
    [GenerateSerializer]
    public record AccountAddedEvent(Account Account);
}