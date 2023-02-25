using Banking.GrainInterfaces;
using Banking.GrainInterfaces.Commands;
using Banking.GrainInterfaces.Managers;
using Banking.GrainInterfaces.Projectors;

namespace Banking.Api
{
    static class RouteGroupBuilderExtensions
    {
        public static RouteGroupBuilder MapCommands(this RouteGroupBuilder group)
        {
            group.MapPost("/createCustomer", async (CreateCustomerCommand command, IClusterClient clusterClient) =>
            {
                ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
                await customerManager.CreateCustomer(command);
            })
            .WithName("CreateCustomer");

            group.MapPost("/updatePrimaryAccountHolder", async (UpdatePrimaryAccountHolderCommand command, IClusterClient clusterClient) =>
            {
                ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
                await customerManager.UpdatePrimaryAccountHolder(command);
            })
            .WithName("UpdatePrimaryAccountHolder");

            group.MapPost("/updatePrimaryResidence", async (UpdatePrimaryResidenceCommand command, IClusterClient clusterClient) =>
            {
                ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
                await customerManager.UpdatePrimaryResidence(command);
            })
            .WithName("UpdatePrimaryResidence");

            group.MapPost("/updateSpouse", async (UpdateSpouseCommand command, IClusterClient clusterClient) =>
            {
                ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
                await customerManager.UpdateSpouse(command);
            })
            .WithName("UpdateSpouse");

            group.MapPost("/updateSpouseResidence", async (UpdateSpouseResidenceCommand command, IClusterClient clusterClient) =>
            {
                ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
                await customerManager.UpdateSpouseResidence(command);
            })
            .WithName("UpdateSpouseResidence");

            group.MapPost("/removeSpouse", async (RemoveSpouseCommand command, IClusterClient clusterClient) =>
            {
                ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
                await customerManager.RemoveSpouse(command);
            })
            .WithName("RemoveSpouse");

            group.MapPost("/updateMailingAddress", async (UpdateMailingAddressCommand command, IClusterClient clusterClient) =>
            {
                ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
                await customerManager.UpdateMailingAddress(command);
            })
            .WithName("UpdateMailingAddress");

            group.MapPost("/addAccount", async (AddAccountCommand command, IClusterClient clusterClient) =>
            {
                ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
                await customerManager.AddAccount(command);
            })
            .WithName("AddAccount");

            group.MapPost("/removeAccount", async (RemoveAccountCommand command, IClusterClient clusterClient) =>
            {
                ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
                await customerManager.RemoveAccount(command);
            })
            .WithName("RemoveAccount");

            group.MapPost("/postTransaction", async (PostTransactionCommand command, IClusterClient clusterClient) =>
            {
                ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
                await customerManager.PostTransaction(command);
            })
            .WithName("PostTransaction");

            return group;
        }

        public static RouteGroupBuilder MapQueries(this RouteGroupBuilder group)
        {
            group.MapGet("/getCustomer", async (string customerId, IClusterClient clusterClient) =>
            {
                ICustomerProjector customerProjector = clusterClient.GetGrain<ICustomerProjector>(customerId);
                return await customerProjector.GetProjection();
            })
            .WithName("GetCustomer");

            group.MapGet("/getCustomers", async (IClusterClient clusterClient) =>
            {
                ICustomersProjector customersProjector = clusterClient.GetGrain<ICustomersProjector>(Constants.AllKey);
                return await customersProjector.GetProjection();
            })
            .WithName("GetCustomers");

            return group;
        }
    }
}