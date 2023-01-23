namespace Banking.Grains.State
{
    [Immutable]
    [GenerateSerializer]
    public record Account(bool IsPrimaryAccount, string AccountType, string AccountNumber, decimal Balance)
    {
        public Account UpdateBalance(decimal amount)
        {
            return new(
                IsPrimaryAccount,
                AccountType,
                AccountNumber,
                Balance + amount);
        }
    }
}
