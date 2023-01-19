namespace Banking.Persistence.Interfaces
{
    public class EventModel
    {
        public required object Data { get; set; }

        public required EventMetadata Metadata { get; set; }
    }
}
