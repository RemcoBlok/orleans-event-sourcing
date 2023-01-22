using Banking.Grains;
using Banking.Persistence.AzureStorage;
using Banking.Persistence.Interfaces;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Providers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("AzureStorage");

// Add services to the container.
builder.Services.AddOrleans(silo =>
{
    silo.UseLocalhostClustering();

    silo.AddLogStorageBasedLogConsistencyProvider();
    silo.AddCustomStorageBasedLogConsistencyProviderAsDefault();

    //silo.AddAzureTableGrainStorageAsDefault((AzureTableStorageOptions options) =>
    //{
    //    options.ConfigureTableServiceClient(connectionString);
    //});

    silo.AddAzureTableGrainStorage(ProviderConstants.DEFAULT_PUBSUB_PROVIDER_NAME, (AzureTableStorageOptions options) =>
    {
        options.ConfigureTableServiceClient(connectionString);
    });

    silo.AddAzureQueueStreams(Constants.StreamProvider, (OptionsBuilder<AzureQueueOptions> optionsBuilder) =>
    {
        optionsBuilder.Configure(options =>
        {
            options.ConfigureQueueServiceClient(connectionString);
        });
    });
});

builder.Services.AddAzureClients(clientFactory =>
{
    clientFactory.AddTableServiceClient(connectionString).WithName(AzureStorageConstants.EventStorageClientName);
});


builder.Services.AddTransient(typeof(IEventStorage<>), typeof(EventStorage<>));
builder.Services.AddTransient(typeof(ICategoryEventsStorage<>), typeof(CategoryEventsStorage<>));
builder.Services.AddTransient(typeof(IProjectionStorage<>), typeof(ProjectionStorage<>));

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.Run();