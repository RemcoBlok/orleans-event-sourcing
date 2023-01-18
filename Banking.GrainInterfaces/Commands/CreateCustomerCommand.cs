namespace Banking.GrainInterfaces.Commands
{
    [Immutable]
    [GenerateSerializer]
    public record CreateCustomerCommand(string CustomerId, Person PrimaryAccountHolder, Address MailingAddress);
}
