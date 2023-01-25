using Banking.GrainInterfaces;
using Banking.GrainInterfaces.Commands;
using Banking.GrainInterfaces.Hubs;
using Banking.GrainInterfaces.Managers;
using Banking.GrainInterfaces.Projectors;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Orleans.Configuration;
using SignalR.Orleans;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.CustomSchemaIds(type => type.FullName);
    o.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date"
    });
});
builder.Services.AddOrleansClient(client =>
{
    client.UseLocalhostClustering();

    //client.AddMemoryStreams(SignalROrleansConstants.SIGNALR_ORLEANS_STREAM_PROVIDER);

    client.AddAzureQueueStreams(SignalROrleansConstants.SIGNALR_ORLEANS_STREAM_PROVIDER, (OptionsBuilder<AzureQueueOptions> optionsBuilder) =>
    {
        optionsBuilder.Configure(options =>
        {
            string? connectionString = builder.Configuration.GetConnectionString("AzureStorage");
            options.ConfigureQueueServiceClient(connectionString);
        });
    });

    client.UseSignalR(configure: null);
});

builder.Services.AddSignalR().AddOrleans();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHub<NotificationHub>("/notificationHub");

app.MapPost("/createCustomer", async (CreateCustomerCommand command, IClusterClient clusterClient) =>
{
    ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
    await customerManager.CreateCustomer(command);
})
.WithName("CreateCustomer")
.WithOpenApi();

app.MapPost("/updatePrimaryAccountHolder", async (UpdatePrimaryAccountHolderCommand command, IClusterClient clusterClient) =>
{
    ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
    await customerManager.UpdatePrimaryAccountHolder(command);
})
.WithName("UpdatePrimaryAccountHolder")
.WithOpenApi();

app.MapPost("/updateSpouse", async (UpdateSpouseCommand command, IClusterClient clusterClient) =>
{
    ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
    await customerManager.UpdateSpouse(command);
})
.WithName("UpdateSpouse")
.WithOpenApi();

app.MapPost("/removeSpouse", async (RemoveSpouseCommand command, IClusterClient clusterClient) =>
{
    ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
    await customerManager.RemoveSpouse(command);
})
.WithName("RemoveSpouse")
.WithOpenApi();

app.MapPost("/updateMailingAddress", async (UpdateMailingAddressCommand command, IClusterClient clusterClient) =>
{
    ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
    await customerManager.UpdateMailingAddress(command);
})
.WithName("UpdateMailingAddress")
.WithOpenApi();

app.MapPost("/addAccount", async (AddAccountCommand command, IClusterClient clusterClient) =>
{
    ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
    await customerManager.AddAccount(command);
})
.WithName("AddAccount")
.WithOpenApi();

app.MapPost("/removeAccount", async (RemoveAccountCommand command, IClusterClient clusterClient) =>
{
    ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
    await customerManager.RemoveAccount(command);
})
.WithName("RemoveAccount")
.WithOpenApi();

app.MapPost("/postTransaction", async (PostTransactionCommand command, IClusterClient clusterClient) =>
{
    ICustomerManager customerManager = clusterClient.GetGrain<ICustomerManager>(command.CustomerId);
    await customerManager.PostTransaction(command);
})
.WithName("PostTransaction")
.WithOpenApi();

app.MapGet("/getCustomer", async (string customerId, IClusterClient clusterClient) =>
{
    ICustomerProjector customerProjector = clusterClient.GetGrain<ICustomerProjector>(customerId);
    return await customerProjector.GetProjection();
})
.WithName("GetCustomer")
.WithOpenApi();

app.MapGet("/getCustomers", async (IClusterClient clusterClient) =>
{
    ICustomersProjector customersProjector = clusterClient.GetGrain<ICustomersProjector>(Constants.AllKey);
    return await customersProjector.GetProjection();
})
.WithName("GetCustomers")
.WithOpenApi();

app.Run();
