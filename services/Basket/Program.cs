var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.AddServiceDefaults();
builder.AddRedisDistributedCache(connectionName: "cache");
builder.Services.AddScoped<BasketService>();

builder.Services.AddHttpClient<CatalogApiClient>(client =>
{
    // Use the service name as the host name to leverage internal DNS in Docker
    client.BaseAddress = new("https+http://catalog");
});

builder.Services.AddMassTransitWithAssemblies(Assembly.GetExecutingAssembly());

builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(
        serviceName: "keycloak",
        realm: "eshop",
        configureOptions: options =>
        {
            options.RequireHttpsMetadata = false;
            options.Audience = "account";
        }
    );
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.MapBasketEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.Run();
