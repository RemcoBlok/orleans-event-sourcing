namespace Banking.Persistence.Interfaces.Models
{
    public readonly record struct CategoryEventsPartitionKey(string CategoryName)
    {
        public override string ToString()
        {
            return $"CategoryEvents-{CategoryName}";
        }
    }
}
