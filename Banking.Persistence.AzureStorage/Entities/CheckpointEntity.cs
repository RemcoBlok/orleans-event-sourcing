using Azure.Data.Tables;
using Azure;

namespace Banking.Persistence.AzureStorage.Entities
{
    internal class CheckpointEntity : ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public int Version { get; set; }
    }
}
