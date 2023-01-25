using Banking.GrainInterfaces.Projections;
using Orleans.Concurrency;

namespace Banking.GrainInterfaces.Projectors
{
    public interface ICustomerProjector : IGrainWithStringKey
    {
        [AlwaysInterleave]
        Task<CustomerProjection> GetProjection();
    }
}
