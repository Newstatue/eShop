

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.AddServiceDefaults();
builder.AddRedisDistributedCache(connectionName: "cache");
builder.Services.AddScoped<IBasketRepository, RedisBasketRepository>();
builder.Services.AddScoped<IBasketService, BasketService>();

var catalogGrpcAddress = builder.Configuration["services:catalog:grpc:0"];
builder.Services.AddGrpcClient<CatalogService.CatalogServiceClient>(options =>
{
    options.Address = new Uri(catalogGrpcAddress!);
});

builder.Services.AddScoped<CatalogGrpcClient>();

// Allow requests from the frontend running on localhost:3001 during development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendDev", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                && uri.Host == "localhost")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddMassTransitWithAssemblies(Assembly.GetExecutingAssembly());

var keycloakSection = builder.Configuration.GetSection("Keycloak");
var keycloakRealm = keycloakSection.GetValue<string>("Realm");

builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(
        serviceName: "keycloak",
        realm: keycloakRealm!,
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

// Enable CORS policy only in Development
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowFrontendDev");
}


app.MapDefaultEndpoints();
app.MapBasketEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.Run();
