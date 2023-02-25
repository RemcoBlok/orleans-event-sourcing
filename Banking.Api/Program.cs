using Banking.Api;
using Banking.GrainInterfaces.Hubs;
using Microsoft.OpenApi.Models;

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

if (builder.Environment.IsDevelopment())
{
    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
    {
        builder.Services.AddOrleansClientDockerDevelopment(builder.Configuration);
    }
    else
    {
        builder.Services.AddOrleansClientDevelopment();
    }
}
else
{
    builder.Services.AddOrleansClientProduction(builder.Configuration);
}

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

app.MapGroup("").MapCommands().WithTags("Commands").WithOpenApi();
app.MapGroup("").MapQueries().WithTags("Queries").WithOpenApi();

app.Run();