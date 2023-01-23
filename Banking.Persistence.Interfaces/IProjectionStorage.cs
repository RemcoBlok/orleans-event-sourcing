using Banking.Persistence.Interfaces.Models;

namespace Banking.Persistence.Interfaces
{
    public interface IProjectionStorage<TState> where TState : notnull
    {
        Task<ProjectionModel<TState>> ReadState(string partitionKey, string rowKey);

        Task<Result> SaveState(string partitionKey, string rowKey, ProjectionModel<TState> projection);
    }
}
