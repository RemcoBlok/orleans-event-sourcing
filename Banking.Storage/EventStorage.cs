using Azure.Data.Tables;
using Microsoft.Extensions.Azure;
using System.Net;

namespace Banking.Storage
{
    public class EventStorage : IEventStorage
    {
        private readonly TableServiceClient _client;
        private const string EventStoreTableName = "EventStore";
        
        public EventStorage(IAzureClientFactory<TableServiceClient> clientFactory)
        {
            _client = clientFactory.CreateClient("EventStorage"); ;
        }

        public async Task<IReadOnlyList<object>> ReadEvents(EventPartitionKey partitionKey)
        {
            if (!await _client.TableExistsAsync(EventStoreTableName).ConfigureAwait(false))
            {
                return Array.Empty<object>();
            }

            TableClient table = _client.GetTableClient(EventStoreTableName);
            
            return await table.QueryAsync<EventEntity>(e => e.PartitionKey == partitionKey.ToString())
                .Select(EventSerialization.DeserializeEvent)
                .Select(e => e.Data)
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<bool> AppendEvents(EventPartitionKey partitionKey, IReadOnlyList<object> events, int expectedVersion)
        {
            if (!events.Any())
            {
                return true;
            }

            TableClient eventStore = _client.GetTableClient(EventStoreTableName);
            await eventStore.CreateIfNotExistsAsync().ConfigureAwait(false);

            int version = expectedVersion;
            IEnumerable<EventEntity> entities = events.Select(ToEventModel)
                .Select(@event => EventSerialization.SerializeEvent(@event, partitionKey.ToString(), ++version));

            List<TableTransactionAction> batch = new();
            foreach (EventEntity entity in entities)
            {
                batch.Add(new TableTransactionAction(TableTransactionActionType.Add, entity));
            }

            try
            {
                await eventStore.SubmitTransactionAsync(batch).ConfigureAwait(false);
                return true;
            }
            catch (TableTransactionFailedException e)
            {
                if (e.Status == (int)HttpStatusCode.Conflict)
                {
                    return false;
                }

                throw;
            }
        }

        private static EventModel ToEventModel(object data)
        {
            return new EventModel
            {
                Data = data,
                Metadata = new EventMetadata
                {
                    TypeName = data.GetType().GetSimpleAssemblyQualifiedName()
                }
            };
        }
    }
}