namespace Banking.Storage
{
    public class EventModel
    {
        public required object Data { get; set; }

        public required EventMetadata Metadata { get; set; }
    }
}
