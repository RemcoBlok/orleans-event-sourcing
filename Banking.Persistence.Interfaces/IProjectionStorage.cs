namespace Banking.Persistence.Interfaces
{
    public interface IProjectionStorage<TState>
    {
        Task<ProjectionModel<TState>> ReadState(string partitionKey, string rowKey);

        Task<Result> SaveState(string partitionKey, string rowKey, ProjectionModel<TState> projection);
    }
}
