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
        private const string TableName = "CategoryEvents";
        private const string CheckpointRowKey = "Checkpoint";

        public CategoryEventsStorage(IAzureClientFactory<TableServiceClient> clientFactory)
        {
            _client = clientFactory.CreateClient("EventStorage"); ;
        }

        public async Task<CheckpointModel> ReadCheckpoint(CategoryEventsPartitionKey partitionKey)
        {
            if (!await _client.TableExistsAsync(TableName).ConfigureAwait(false))
            {
                return new(0, null);
            }

            TableClient table = _client.GetTableClient(TableName);

            NullableResponse<CheckpointEntity> response = await table.GetEntityIfExistsAsync<CheckpointEntity>(partitionKey.ToString(), CheckpointRowKey);
            if (response.HasValue)
            {
                return new(response.Value.Version, response.Value.ETag.ToString());
            }

            return new(0, null);
        }

        public async Task<Result> AppendEvents(CategoryEventsPartitionKey partitionKey, IReadOnlyList<object> events, CheckpointModel checkpoint)
        {
            TableClient eventStore = _client.GetTableClient(TableName);
            await eventStore.CreateIfNotExistsAsync().ConfigureAwait(false);

            int version = checkpoint.Version;
            IEnumerable<EventEntity> entities = events.Select(ToEventModel)
                .Select(@event => @event.SerializeEvent(partitionKey.ToString(), ++version));

            List<TableTransactionAction> batch = new();
            foreach (EventEntity entity in entities)
            {
                batch.Add(new TableTransactionAction(TableTransactionActionType.Add, entity));
            }

            CheckpointEntity checkpointEntity = CreateCheckpointEntity(partitionKey.ToString(), version);
            AddCheckpointToBatch(batch, checkpointEntity, checkpoint);

            try
            {
                Response<IReadOnlyList<Response>> transactionResponse = await eventStore.SubmitTransactionAsync(batch).ConfigureAwait(false);

                Response checkpointResponse = transactionResponse.Value[^1];
                return new(false, checkpointResponse.Headers.ETag.ToString());
            }
            catch (RequestFailedException e)
            {
                if (e.Status == (int)HttpStatusCode.Conflict)
                {
                    return new(true, null);
                }

                throw;
            }
        }

        private static void AddCheckpointToBatch(List<TableTransactionAction> batch, CheckpointEntity checkpointEntity, CheckpointModel checkpoint)
        {
            if (string.IsNullOrEmpty(checkpoint.ETag))
            {
                batch.Add(new TableTransactionAction(TableTransactionActionType.Add, checkpointEntity));
            }
            else
            {
                checkpointEntity.ETag = new ETag(checkpoint.ETag);
                batch.Add(new TableTransactionAction(TableTransactionActionType.UpdateReplace, checkpointEntity, checkpointEntity.ETag));
            }
        }

        private static CheckpointEntity CreateCheckpointEntity(string partitionKey, int version)
        {
            return new()
            {
                PartitionKey = partitionKey,
                RowKey = CheckpointRowKey,
                Version = version
            };
        }

        private static EventModel ToEventModel(object data)
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