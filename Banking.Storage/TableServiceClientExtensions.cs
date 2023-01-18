using Azure.Data.Tables;

namespace Banking.Storage
{
    internal static class TableServiceClientExtensions
    {
        public static ValueTask<bool> TableExistsAsync(this TableServiceClient client, string tableName)
        {
            return client.QueryAsync(filter: $"TableName eq '{tableName}'").AnyAsync();
        }
    }
}
