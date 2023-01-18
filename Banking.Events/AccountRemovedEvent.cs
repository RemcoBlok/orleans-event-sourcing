namespace Banking.Events
{
    [Immutable]
    [GenerateSerializer]
    public record AccountRemovedEvent(string AccountNumber);
}