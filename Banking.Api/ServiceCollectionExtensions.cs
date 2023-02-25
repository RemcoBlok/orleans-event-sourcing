using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using SignalR.Orleans;

namespace Banking.Api
{
    static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrleansClientDevelopment(this IServiceCollection services)
        {
            services.AddOrleansClient(client =>
            {
                client.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = Environments.Development;
                    options.ServiceId = "Banking";
                });

                client.UseLocalhostClustering();

                client.UseSignalR(configure: null);
            });

            return services;
        }

        public static IServiceCollection AddOrleansClientDockerDevelopment(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOrleansClient(client =>
            {
                client.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = Environments.Development + "Container";
                    options.ServiceId = "Banking";
                });

                string redisConnectionString = Environment.GetEnvironmentVariable("ORLEANS_REDIS_CONNECTION_STRING")
                    ?? configuration.GetConnectionString("ORLEANS_REDIS_CONNECTION_STRING")
                    ?? "redis:6379";

                client.UseRedisClustering(redisConnectionString);

                client.UseSignalR(configure: null);
            });

            return services;
        }

        public static IServiceCollection AddOrleansClientProduction(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOrleansClient(client =>
            {
                client.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = Environments.Production;
                    options.ServiceId = "Banking";
                });

                string? azureStorageConnectionString = Environment.GetEnvironmentVariable("ORLEANS_AZURE_STORAGE_CONNECTION_STRING")
                    ?? configuration.GetConnectionString("ORLEANS_AZURE_STORAGE_CONNECTION_STRING");

                client.UseAzureStorageClustering((AzureStorageGatewayOptions options) =>
                {
                    options.ConfigureTableServiceClient(azureStorageConnectionString);
                });

                string? eventHubConnectionString = Environment.GetEnvironmentVariable("ORLEANS_EVENT_HUB_CONNECTION_STRING")
                    ?? configuration.GetConnectionString("ORLEANS_EVENT_HUB_CONNECTION_STRING");

                client.AddEventHubStreams(SignalROrleansConstants.SIGNALR_ORLEANS_STREAM_PROVIDER, (IClusterClientEventHubStreamConfigurator configurator) =>
                {
                    configurator.ConfigureEventHub(builder => builder.Configure(options =>
                    {
                        options.ConfigureEventHubConnection(
                            eventHubConnectionString,
                            "Banking",
                            "$Default");
                    }));
                });

                client.UseSignalR(configure: null);
            });

            return services;
        }
    }
}