var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.AddServiceDefaults();

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

builder.AddOllamaApiClient("chat")
    .AddChatClient();

builder.AddOllamaApiClient("embedding")
    .AddEmbeddingGenerator();

builder.Services.AddSingleton<InMemoryVectorStore>();
builder.Services.AddInMemoryVectorStoreRecordCollection<int, ProductVector>("products");

builder.AddNpgsqlDbContext<ProductDbContext>("catalogdb");
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProductAIService>();
builder.Services.AddMassTransitWithAssemblies(Assembly.GetExecutingAssembly());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMigration();

// Enable CORS policy only in Development
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowFrontendDev");
}

app.MapDefaultEndpoints();
app.MapProductEndpoints();

app.UseHttpsRedirection();

app.Run();

