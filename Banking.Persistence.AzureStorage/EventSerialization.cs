using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;
using Banking.Persistence.Interfaces;

namespace Banking.Persistence.AzureStorage
{
    internal static class EventSerialization
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static EventModel<TEventBase> DeserializeEvent<TEventBase>(this EventEntity e) where TEventBase : notnull
        {
            EventMetadata metadata = JsonSerializer.Deserialize<EventMetadata>(e.Metadata, Options)!;

            TEventBase data = (TEventBase)JsonSerializer.Deserialize(
                e.Data,
                TypeCache.GetType(metadata.TypeName),
                Options)!;

            return new()
            {
                Data = data,
                Metadata = metadata
            };
        }

        public static EventEntity SerializeEvent<TEventBase>(this EventModel<TEventBase> @event, string partitionKey, int version) where TEventBase : notnull
        {
            TEventBase data = @event.Data;
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
