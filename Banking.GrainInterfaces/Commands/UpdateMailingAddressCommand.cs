namespace Banking.GrainInterfaces.Commands
{
    [Immutable]
    [GenerateSerializer]
    public record UpdateMailingAddressCommand(string CustomerId, Address MailingAddress);
}
