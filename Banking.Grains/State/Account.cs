namespace Banking.Grains.State
{
    public class Account
    {
        public required bool IsPrimaryAccount { get; init; }
        public required string AccountType { get; init; }
        public required string AccountNumber { get; init; }
        public required decimal Balance { get; init; }

        public Account UpdateBalance(decimal amount)
        {
            return new Account { 
                IsPrimaryAccount = IsPrimaryAccount,
                AccountType = AccountType,
                AccountNumber = AccountNumber,
                Balance = Balance + amount
            };
        }
    }
}
