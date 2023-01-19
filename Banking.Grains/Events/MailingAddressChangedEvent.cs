namespace Banking.Grains.Events
{
    [Immutable]
    [GenerateSerializer]
    public record MailingAddressChangedEvent(Address MailingAddress);
}