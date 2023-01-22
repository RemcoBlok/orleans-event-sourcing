namespace Banking.Persistence.Interfaces
{
    public class ProjectionModel<TState>
    {
        public required TState Data { get; set; }

        public required ProjectionMetadata Metadata { get; set; }

        public string? ETag { get; set; }
    }
}
