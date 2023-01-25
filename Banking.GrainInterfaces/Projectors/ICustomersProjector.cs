using Banking.GrainInterfaces.Projections;
using Orleans.Concurrency;

namespace Banking.GrainInterfaces.Projectors
{
    public interface ICustomersProjector : IGrainWithStringKey
    {
        [AlwaysInterleave]
        Task<CustomersProjection> GetProjection();
    }
}
