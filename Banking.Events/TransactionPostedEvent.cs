namespace Banking.Events
{
    [Immutable]
    [GenerateSerializer]
    public record TransactionPostedEvent(string AccountNumber, decimal Amount);
}