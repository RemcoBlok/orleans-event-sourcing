using Banking.Persistence.Interfaces;
using Banking.Persistence.Interfaces.Models;

namespace Banking.Persistence.Redis
{
    public class ProjectionStorage<TState> : IProjectionStorage<TState> where TState : notnull, new()
    {
        public Task<ProjectionModel<TState>> ReadState(string partitionKey, string rowKey)
        {
            throw new NotImplementedException();
        }

        public Task<Result> SaveState(string partitionKey, string rowKey, ProjectionModel<TState> projection)
        {
            throw new NotImplementedException();
        }
    }
}
