namespace Banking.GrainInterfaces.Projections
{
    [Immutable]
    [GenerateSerializer]
    public record CustomersProjection(int CustomerCount, int AccountCount, int TransactionCount, decimal AccountBalanceSum);
}
