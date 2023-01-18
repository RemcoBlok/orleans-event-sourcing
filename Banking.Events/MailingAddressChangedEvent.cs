namespace Banking.Events
{
    [Immutable]
    [GenerateSerializer]
    public record MailingAddressChangedEvent(Address MailingAddress);
}