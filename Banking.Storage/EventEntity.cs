using Azure.Data.Tables;
using Azure;

namespace Banking.Storage
{
    internal class EventEntity : ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public byte[]? Data { get; set; }
        public byte[]? Metadata { get; set; }
    }
}
