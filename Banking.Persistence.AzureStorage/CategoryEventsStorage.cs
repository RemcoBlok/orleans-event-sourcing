using Azure;
using Azure.Data.Tables;
using Banking.Persistence.Interfaces;
using Microsoft.Extensions.Azure;
using System.Net;

namespace Banking.Persistence.AzureStorage
{
    public class CategoryEventsStorage : ICategoryEventsStorage
    {
        private readonly TableServiceClient _client;
        private const string EventStoreTableName = "CategoryEvents";
        private const string CategoryEventsRowKey = "Checkpoint";

        public CategoryEventsStorage(IAzureClientFactory<TableServiceClient> clientFactory)
        {
            _client = clientFactory.CreateClient("EventStorage"); ;
        }

        public async Task<CategoryEventsModel> ReadEvents(CategoryEventsPartitionKey partitionKey)
        {
            if (!await _client.TableExistsAsync(EventStoreTableName).ConfigureAwait(false))
            {
                return new CategoryEventsModel(0, null);
            }

            TableClient table = _client.GetTableClient(EventStoreTableName);

            NullableResponse<CategoryEventsEntity> response = await table.GetEntityIfExistsAsync<CategoryEventsEntity>(partitionKey.ToString(), CategoryEventsRowKey);
            if (response.HasValue)
            {
                return new CategoryEventsModel(response.Value.Count, response.Value.ETag.ToString());
            }

            return new CategoryEventsModel(0, null);
        }

        public async Task<AppendCategoryEventsResult> AppendEvents(CategoryEventsPartitionKey partitionKey, IReadOnlyList<object> events, CategoryEventsModel expected)
        {
            TableClient eventStore = _client.GetTableClient(EventStoreTableName);
            await eventStore.CreateIfNotExistsAsync().ConfigureAwait(false);

            int version = expected.Version;
            IEnumerable<EventEntity> entities = events.Select(ToEventModel)
                .Select(@event => @event.SerializeEvent(partitionKey.ToString(), ++version));

            List<TableTransactionAction> batch = new();
            foreach (EventEntity entity in entities)
            {
                batch.Add(new TableTransactionAction(TableTransactionActionType.Add, entity));
            }

            CategoryEventsEntity categoryEventsEntity = CreateCategoryEventsEntity(partitionKey, version);
            AddCategoryEventsEntityToBatch(batch, categoryEventsEntity, expected);

            try
            {
                Response<IReadOnlyList<Response>> transactionResponse = await eventStore.SubmitTransactionAsync(batch).ConfigureAwait(false);
                Response response = transactionResponse.Value[transactionResponse.Value.Count - 1];
                return new AppendCategoryEventsResult(false, response.Headers.ETag.ToString());
            }
            catch (TableTransactionFailedException e)
            {
                if (e.Status == (int)HttpStatusCode.Conflict)
                {
                    return new AppendCategoryEventsResult(true, null);
                }

                throw;
            }
        }

        private static void AddCategoryEventsEntityToBatch(List<TableTransactionAction> batch, CategoryEventsEntity categoryEventsEntity, CategoryEventsModel expected)
        {
            if (string.IsNullOrEmpty(expected.ETag))
            {
                batch.Add(new TableTransactionAction(TableTransactionActionType.Add, categoryEventsEntity));
            }
            else
            {
                categoryEventsEntity.ETag = new ETag(expected.ETag);
                batch.Add(new TableTransactionAction(TableTransactionActionType.UpdateReplace, categoryEventsEntity, categoryEventsEntity.ETag));
            }
        }

        private static CategoryEventsEntity CreateCategoryEventsEntity(CategoryEventsPartitionKey partitionKey, int version)
        {
            return new()
            {
                PartitionKey = partitionKey.ToString(),
                RowKey = CategoryEventsRowKey,
                Count = version
            };
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