using Banking.GrainInterfaces.Projections;
using Orleans.Concurrency;

namespace Banking.GrainInterfaces
{
    public interface ICustomerProjector : IGrainWithStringKey
    {
        [AlwaysInterleave]
        Task<CustomerProjection> GetProjection();
    }
}
