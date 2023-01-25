namespace Banking.GrainInterfaces.Hubs
{
    public interface INotificationClient
    {
        Task NotifyCustomerProjectionUpdated(string customerId);
        Task NotifyCustomersProjectionUpdated();
    }
}
