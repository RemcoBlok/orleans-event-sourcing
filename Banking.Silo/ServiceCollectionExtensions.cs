using Banking.GrainInterfaces;
using Banking.GrainInterfaces.Hubs;
using Banking.Persistence.AzureStorage;
using Banking.Persistence.Interfaces;
using Microsoft.Extensions.Azure;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Orleans.Persistence;
using Orleans.Providers;
using SignalR.Orleans;
using StackExchange.Redis;

namespace Banking.Silo
{
    static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrleansDevelopment(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOrleans(silo =>
            {
                silo.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = Environments.Development;
                    options.ServiceId = "Banking";
                });

                string redisConnectionString = configuration.GetConnectionString("ORLEANS_REDIS_CONNECTION_STRING")
                    ?? "localhost:6379";

                silo.UseLocalhostClustering();

                silo.AddLogStorageBasedLogConsistencyProvider(Grains.Constants.EventStorageName);
                silo.AddLogStorageBasedLogConsistencyProvider(Grains.Constants.CategoryEventStorageName);
                silo.AddStateStorageBasedLogConsistencyProvider(Grains.Constants.ProjectionStorageName);

                silo.AddRedisGrainStorageAsDefault((RedisStorageOptions options) =>
                {
                    options.ConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString);
                });

                //silo.AddRedisGrainStorage(SignalROrleansConstants.SIGNALR_ORLEANS_STORAGE_PROVIDER, (RedisStorageOptions options) =>
                //{
                //    options.ConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString);
                //});

                //silo.AddRedisGrainStorage(ProviderConstants.DEFAULT_PUBSUB_PROVIDER_NAME, (RedisStorageOptions options) =>
                //{
                //    options.ConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString);
                //});

                silo.AddMemoryStreams(Grains.Constants.StreamProviderName); // Redis streaming?

                silo.UseSignalR();
                silo.RegisterHub<NotificationHub>();
            });

            return services;
        }

        public static IServiceCollection AddOrleansDockerDevelopment(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOrleans(silo =>
            {
                silo.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = Environments.Development + "Container";
                    options.ServiceId = "Banking";
                });

                string redisConnectionString = Environment.GetEnvironmentVariable("ORLEANS_REDIS_CONNECTION_STRING")
                    ?? configuration.GetConnectionString("ORLEANS_REDIS_CONNECTION_STRING")
                    ?? "redis:6379";

                silo.UseRedisClustering(redisConnectionString);

                silo.AddLogStorageBasedLogConsistencyProvider(Grains.Constants.EventStorageName);
                silo.AddLogStorageBasedLogConsistencyProvider(Grains.Constants.CategoryEventStorageName);
                silo.AddStateStorageBasedLogConsistencyProvider(Grains.Constants.ProjectionStorageName);

                silo.AddRedisGrainStorageAsDefault((RedisStorageOptions options) =>
                {
                    options.ConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString);
                });

                //silo.AddRedisGrainStorage(SignalROrleansConstants.SIGNALR_ORLEANS_STORAGE_PROVIDER, (RedisStorageOptions options) =>
                //{
                //    options.ConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString);
                //});

                //silo.AddRedisGrainStorage(ProviderConstants.DEFAULT_PUBSUB_PROVIDER_NAME, (RedisStorageOptions options) =>
                //{
                //    options.ConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString);
                //});

                silo.AddMemoryStreams(Grains.Constants.StreamProviderName); // Redis streaming?

                silo.UseSignalR();
                silo.RegisterHub<NotificationHub>();
            });

            return services;
        }

        public static IServiceCollection AddOrleansProduction(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOrleans(silo =>
            {
                silo.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = Environments.Production;
                    options.ServiceId = "LEDMatrix";
                });

                string? azureStorageConnectionString = configuration.GetConnectionString("ORLEANS_AZURE_STORAGE_CONNECTION_STRING")
                ?? configuration.GetConnectionString("ORLEANS_AZURE_STORAGE_CONNECTION_STRING");

                silo.UseAzureStorageClustering((AzureStorageClusteringOptions options) =>
                {
                    options.ConfigureTableServiceClient(azureStorageConnectionString);
                });

                silo.AddLogStorageBasedLogConsistencyProvider();
                silo.AddCustomStorageBasedLogConsistencyProvider(Grains.Constants.EventStorageName);
                silo.AddCustomStorageBasedLogConsistencyProvider(Grains.Constants.CategoryEventStorageName);
                silo.AddCustomStorageBasedLogConsistencyProvider(Grains.Constants.ProjectionStorageName);

                silo.AddAzureTableGrainStorage(SignalROrleansConstants.SIGNALR_ORLEANS_STORAGE_PROVIDER, (AzureTableStorageOptions options) =>
                {
                    options.ConfigureTableServiceClient(azureStorageConnectionString);
                });

                silo.AddAzureTableGrainStorage(ProviderConstants.DEFAULT_PUBSUB_PROVIDER_NAME, (AzureTableStorageOptions options) =>
                {
                    options.ConfigureTableServiceClient(azureStorageConnectionString);
                });

                string? eventHubConnectionString = Environment.GetEnvironmentVariable("ORLEANS_EVENT_HUB_CONNECTION_STRING")
                    ?? configuration.GetConnectionString("ORLEANS_EVENT_HUB_CONNECTION_STRING");

                silo.AddEventHubStreams(SignalROrleansConstants.SIGNALR_ORLEANS_STREAM_PROVIDER, (ISiloEventHubStreamConfigurator configurator) =>
                {
                    configurator.ConfigureEventHub(builder => builder.Configure(options =>
                    {
                        options.ConfigureEventHubConnection(
                            eventHubConnectionString,
                            "Banking",
                            "$Default");
                    }));

                    configurator.UseAzureTableCheckpointer(builder => builder.Configure(options =>
                    {
                        options.ConfigureTableServiceClient(azureStorageConnectionString);
                        options.PersistInterval = TimeSpan.FromSeconds(10);
                    }));
                });

                silo.AddEventHubStreams(Grains.Constants.StreamProviderName, (ISiloEventHubStreamConfigurator configurator) =>
                {
                    configurator.ConfigureEventHub(builder => builder.Configure(options =>
                    {
                        options.ConfigureEventHubConnection(
                            eventHubConnectionString,
                            "Banking",
                            "$Default");
                    }));

                    configurator.UseAzureTableCheckpointer(builder => builder.Configure(options =>
                    {
                        options.ConfigureTableServiceClient(azureStorageConnectionString);
                        options.PersistInterval = TimeSpan.FromSeconds(10);
                    }));
                });

                silo.UseSignalR();
                silo.RegisterHub<NotificationHub>();
            });

            return services;
        }

        public static IServiceCollection AddStorageDevelopment(this IServiceCollection services)
        {            
            services.AddTransient(typeof(IEventStorage<>), typeof(Persistence.Redis.EventStorage<>));
            services.AddTransient(typeof(ICategoryEventsStorage<>), typeof(Persistence.Redis.CategoryEventsStorage<>));
            services.AddTransient(typeof(IProjectionStorage<>), typeof(Persistence.Redis.ProjectionStorage<>));

            return services;
        }

        public static IServiceCollection AddStorageProduction(this IServiceCollection services, IConfiguration configuration)
        {
            string? azureStorageConnectionString = configuration.GetConnectionString("ORLEANS_AZURE_STORAGE_CONNECTION_STRING")
                ?? configuration.GetConnectionString("ORLEANS_AZURE_STORAGE_CONNECTION_STRING");

            services.AddAzureClients(clientFactory =>
            {
                clientFactory.AddTableServiceClient(azureStorageConnectionString).WithName(AzureStorageConstants.EventStorageClientName);
            });

            services.AddTransient(typeof(IEventStorage<>), typeof(EventStorage<>));
            services.AddTransient(typeof(ICategoryEventsStorage<>), typeof(CategoryEventsStorage<>));
            services.AddTransient(typeof(IProjectionStorage<>), typeof(ProjectionStorage<>));

            return services;
        }
    }
}