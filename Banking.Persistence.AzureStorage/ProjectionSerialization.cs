using System.Text.Json.Serialization;
using System.Text.Json;
using Banking.Persistence.Interfaces;

namespace Banking.Persistence.AzureStorage
{
    internal static class ProjectionSerialization
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static ProjectionModel<TState> DeserializeProjection<TState>(this ProjectionEntity projection) where TState : notnull
        {
            ProjectionMetadata metadata = JsonSerializer.Deserialize<ProjectionMetadata>(projection.Metadata, Options)!;

            TState data = JsonSerializer.Deserialize<TState>(projection.Data, Options)!;

            return new ProjectionModel<TState>
            {
                Data = data,
                Metadata = metadata,
                ETag = projection.ETag.ToString()
            };
        }

        public static ProjectionEntity SerializeProjection<TState>(this ProjectionModel<TState> projection, string partitionKey, string rowKey) where TState : notnull
        {
            TState data = projection.Data;
            ProjectionMetadata metadata = projection.Metadata;

            return new ProjectionEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                Data = JsonSerializer.SerializeToUtf8Bytes(data, Options),
                Metadata = JsonSerializer.SerializeToUtf8Bytes(metadata, Options)
            };
        }
    }
}
