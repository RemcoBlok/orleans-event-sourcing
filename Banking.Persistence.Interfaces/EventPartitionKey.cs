namespace Banking.Persistence.Interfaces
{
    public readonly record struct EventPartitionKey(string CategoryName, string StreamId)
    {
        public override string ToString()
        {
            return $"{CategoryName}-{StreamId}";
        }
    }
}
