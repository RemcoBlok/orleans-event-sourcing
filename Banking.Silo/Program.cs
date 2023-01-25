using Banking.GrainInterfaces.Hubs;
using Banking.Persistence.AzureStorage;
using Banking.Persistence.Interfaces;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Providers;
using SignalR.Orleans;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("AzureStorage");

// Add services to the container.
builder.Host.UseOrleans(silo =>
{
    silo.UseLocalhostClustering();

    silo.AddLogStorageBasedLogConsistencyProvider();
    silo.AddCustomStorageBasedLogConsistencyProviderAsDefault();

    //silo.AddMemoryGrainStorage(SignalROrleansConstants.SIGNALR_ORLEANS_STORAGE_PROVIDER);
    //silo.AddMemoryGrainStorage(ProviderConstants.DEFAULT_PUBSUB_PROVIDER_NAME);
    //silo.AddMemoryStreams(SignalROrleansConstants.SIGNALR_ORLEANS_STREAM_PROVIDER);
    //silo.AddMemoryStreams(Banking.Grains.Constants.StreamProvider);

    silo.AddAzureTableGrainStorage(SignalROrleansConstants.SIGNALR_ORLEANS_STORAGE_PROVIDER, (AzureTableStorageOptions options) =>
    {
        options.ConfigureTableServiceClient(connectionString);
    });

    silo.AddAzureTableGrainStorage(ProviderConstants.DEFAULT_PUBSUB_PROVIDER_NAME, (AzureTableStorageOptions options) =>
    {
        options.ConfigureTableServiceClient(connectionString);
    });

    //silo.AddPersistentStreams(SignalROrleansConstants.SIGNALR_ORLEANS_STREAM_PROVIDER, AzureQueueAdapterFactory.Create, c =>
    //{
    //    c.Configure<AzureQueueOptions>(optionsBuilder =>
    //    {
    //        optionsBuilder.Configure(options =>
    //        {
    //            options.ConfigureQueueServiceClient(connectionString);
    //        });
    //    });
    //});

    silo.AddAzureQueueStreams(SignalROrleansConstants.SIGNALR_ORLEANS_STREAM_PROVIDER, (OptionsBuilder<AzureQueueOptions> optionsBuilder) =>
    {
        optionsBuilder.Configure(options =>
        {
            options.ConfigureQueueServiceClient(connectionString);
        });
    });

    silo.AddAzureQueueStreams(Banking.Grains.Constants.StreamProvider, (OptionsBuilder<AzureQueueOptions> optionsBuilder) =>
    {
        optionsBuilder.Configure(options =>
        {
            options.ConfigureQueueServiceClient(connectionString);
        });
    });

    silo.UseSignalR();
    silo.RegisterHub<NotificationHub>();
});

builder.Services.AddSignalR().AddOrleans();

builder.Services.AddAzureClients(clientFactory =>
{
    clientFactory.AddTableServiceClient(connectionString).WithName(AzureStorageConstants.EventStorageClientName);
});

builder.Services.AddTransient(typeof(IEventStorage<>), typeof(EventStorage<>));
builder.Services.AddTransient(typeof(ICategoryEventsStorage<>), typeof(CategoryEventsStorage<>));
builder.Services.AddTransient(typeof(IProjectionStorage<>), typeof(ProjectionStorage<>));

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

await app.RunAsync();