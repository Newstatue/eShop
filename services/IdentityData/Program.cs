using System.Reflection;

using IdentityData.Data;
using IdentityData.GrpcServices;
using IdentityData.Services;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<IdentityDbContext>("identitydb");
builder.Services.AddScoped<IUserEventHandler, UserEventHandler>();
builder.Services.AddMassTransitWithAssemblies(Assembly.GetExecutingAssembly());
builder.Services.AddGrpc();
builder.Services.AddGrpcHealthChecks();

var app = builder.Build();

app.UseMigration();

app.MapGrpcService<IdentityDataGrpcService>();
app.MapGrpcHealthChecksService();
app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Text(
    "IdentityData gRPC service is running. Use a gRPC client to communicate with it.",
    "text/plain"));

app.Run();


