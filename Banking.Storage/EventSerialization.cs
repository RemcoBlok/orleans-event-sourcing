using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Banking.Storage
{
    internal static class EventSerialization
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static EventModel DeserializeEvent(this EventEntity e)
        {
            EventMetadata metadata = JsonSerializer.Deserialize<EventMetadata>(e.Metadata, Options)!;

            object data = JsonSerializer.Deserialize(
                e.Data,
                TypeCache.GetType(metadata.TypeName),
                Options)!;

            return new EventModel
            {
                Data = data,
                Metadata = metadata
            };
        }

        public static EventEntity SerializeEvent(this EventModel @event, string partitionKey, int version)
        {
            object data = @event.Data;
            EventMetadata metadata = @event.Metadata;

            return new EventEntity
            {
                PartitionKey = partitionKey,
                RowKey = version.ToString("D19", CultureInfo.InvariantCulture),
                Data = JsonSerializer.SerializeToUtf8Bytes(data, data.GetType(), Options),
                Metadata = JsonSerializer.SerializeToUtf8Bytes(metadata, Options)
            };
        }
    }
}
