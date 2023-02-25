using Banking.Silo;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
if (builder.Environment.IsDevelopment())
{
    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
    {
        builder.Services.AddOrleansDockerDevelopment(builder.Configuration);
        builder.Services.AddStorageDevelopment();
    }
    else
    {
        builder.Services.AddOrleansDevelopment(builder.Configuration);
        builder.Services.AddStorageDevelopment();
    }
}
else
{
    builder.Services.AddOrleansProduction(builder.Configuration);
    builder.Services.AddStorageProduction(builder.Configuration);
}

builder.Services.AddSignalR().AddOrleans();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.Run();