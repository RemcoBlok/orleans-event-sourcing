namespace Banking.Grains.State
{
    public class CategoryEventsState
    {
        public string? ETag { get; private set; }

        public void Apply(string eTag)
        {
            ETag = eTag;
        }
    }
}
