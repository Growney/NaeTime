using EventStore.Helpers;
using NaeTime.Server.EventStore.Projections;
using NaeTime.Server.WebAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
// Add services to the container.

builder.Services.AddEventStoreConsumers();
builder.Services.AddControllers();

builder.Services.AddProjections(projectionBuilder =>
{
    projectionBuilder.AddAllStreamProjectionHandler(EventStore.Client.FromAll.Start, x =>
    {
        x.AddSingletonProjection<PassProjection>();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();
app.MapHub<NodeHub>("/NodeHub");

app.Run();
