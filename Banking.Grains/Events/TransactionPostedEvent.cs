namespace Banking.Grains.Events
{
    [Immutable]
    [GenerateSerializer]
    public record TransactionPostedEvent(string AccountNumber, decimal Amount);
}