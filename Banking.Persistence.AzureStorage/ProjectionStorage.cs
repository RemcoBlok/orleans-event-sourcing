using Azure;
using Azure.Data.Tables;
using Banking.Persistence.AzureStorage.Entities;
using Banking.Persistence.AzureStorage.Serialization;
using Banking.Persistence.Interfaces;
using Banking.Persistence.Interfaces.Models;
using Microsoft.Extensions.Azure;
using System.Net;

namespace Banking.Persistence.AzureStorage
{
    public class ProjectionStorage<TState> : IProjectionStorage<TState> where TState : notnull, new()
    {
        private readonly TableServiceClient _client;
        private const string TableName = "Projections";

        public ProjectionStorage(IAzureClientFactory<TableServiceClient> clientFactory)
        {
            _client = clientFactory.CreateClient(AzureStorageConstants.EventStorageClientName); ;
        }

        public async Task<ProjectionModel<TState>> ReadState(string partitionKey, string rowKey)
        {
            if (!await _client.TableExistsAsync(TableName).ConfigureAwait(false))
            {
                return ProjectionSerialization.ToProjectionModel(new TState());
            }

            TableClient table = _client.GetTableClient(TableName);

            NullableResponse<ProjectionEntity> response = await table.GetEntityIfExistsAsync<ProjectionEntity>(partitionKey, rowKey);
            if (response.HasValue)
            {
                return ProjectionSerialization.DeserializeProjection<TState>(response.Value);
            }

            return ProjectionSerialization.ToProjectionModel(new TState());
        }

        public async Task<Result> SaveState(string partitionKey, string rowKey, ProjectionModel<TState> projection)
        {
            TableClient table = _client.GetTableClient(TableName);
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            ProjectionEntity entity = ProjectionSerialization.SerializeProjection(projection, partitionKey, rowKey);

            try
            {
                Response response;
                if (string.IsNullOrEmpty(projection.ETag))
                {
                    response = await table.AddEntityAsync(entity);
                }
                else
                {
                    entity.ETag = new ETag(projection.ETag);
                    response = await table.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);
                }

                return new(false, response.Headers.ETag.ToString());
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
    }
}
