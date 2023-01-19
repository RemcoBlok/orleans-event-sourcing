namespace Banking.Grains.Events
{
    [Immutable]
    [GenerateSerializer]
    public record CustomerCreatedEvent(string CustomerId, Person PrimaryAccountHolder, Address MailingAddress);
}