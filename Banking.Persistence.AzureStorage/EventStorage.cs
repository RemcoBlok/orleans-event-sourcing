using Azure;
using Azure.Data.Tables;
using Banking.Persistence.Interfaces;
using Microsoft.Extensions.Azure;
using System.Net;

namespace Banking.Persistence.AzureStorage
{
    public class EventStorage<TEventBase> : IEventStorage<TEventBase> where TEventBase : notnull
    {
        private readonly TableServiceClient _client;
        private const string TableName = "EventStore";

        public EventStorage(IAzureClientFactory<TableServiceClient> clientFactory)
        {
            _client = clientFactory.CreateClient(AzureStorageConstants.EventStorageClientName); ;
        }

        public async Task<IReadOnlyList<TEventBase>> ReadEvents(EventPartitionKey partitionKey)
        {
            if (!await _client.TableExistsAsync(TableName).ConfigureAwait(false))
            {
                return Array.Empty<TEventBase>();
            }

            TableClient table = _client.GetTableClient(TableName);

            return await table.QueryAsync<EventEntity>(e => e.PartitionKey == partitionKey.ToString())
                .Select(EventSerialization.DeserializeEvent<TEventBase>)
                .Select(e => e.Data)
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<bool> AppendEvents(EventPartitionKey partitionKey, IReadOnlyList<TEventBase> events, int expectedVersion)
        {
            if (!events.Any())
            {
                return true;
            }

            TableClient eventStore = _client.GetTableClient(TableName);
            await eventStore.CreateIfNotExistsAsync().ConfigureAwait(false);

            int version = expectedVersion;
            IEnumerable<EventEntity> entities = events.Select(ToEventModel)
                .Select(@event => @event.SerializeEvent(partitionKey.ToString(), ++version));

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
            catch (RequestFailedException e)
            {
                if (e.Status == (int)HttpStatusCode.Conflict)
                {
                    return false;
                }

                throw;
            }
        }

        private static EventModel<TEventBase> ToEventModel(TEventBase data)
        {
            return new()
            {
                Data = data,
                Metadata = new()
                {
                    TypeName = data.GetType().GetSimpleAssemblyQualifiedName()
                }
            };
        }
    }
}