using Azure.Data.Tables;
using Azure;

namespace Banking.Persistence.AzureStorage
{
    internal class CategoryEventsEntity : ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public int Count { get; set; }
    }
}
