namespace Banking.Persistence.Interfaces
{
    public readonly record struct CategoryEventsPartitionKey(string CategoryName)
    {
        public override string ToString()
        {
            return $"CategoryEvents-{CategoryName}";
        }
    }
}
