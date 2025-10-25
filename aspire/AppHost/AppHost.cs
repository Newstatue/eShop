var builder = DistributedApplication.CreateBuilder(args);

//微服务
var postgres = builder
    .AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var catalogDb =
    postgres.AddDatabase("catalogdb");

var cache = builder
    .AddRedis("cache")
    .WithRedisInsight()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var rabbitMq = builder
    .AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var keycloak = builder
    .AddKeycloak("keycloak")
    .WithHttpsEndpoint(8443, 8443)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var ollma = builder
    .AddOllama("ollama", 11434)
    .WithDataVolume()
    .WithImageTag("0.12.6")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithOpenWebUI();

var chat = ollma
    .AddModel("chat", "qwen3:0.6b");

var embedding = ollma
    .AddModel("embedding", "qwen3-embedding:0.6b");

//项目
var catalog = builder
    .AddProject<Projects.Catalog>("catalog")
    .WithReference(catalogDb)
    .WithReference(rabbitMq)
    .WithReference(chat)
    .WithReference(embedding)
    .WaitFor(catalogDb)
    .WaitFor(rabbitMq)
    .WaitFor(chat)
    .WaitFor(embedding);

var basket = builder
    .AddProject<Projects.Basket>("basket")
    .WithReference(cache)
    .WithReference(catalog)
    .WithReference(rabbitMq)
    .WithReference(keycloak)
    .WaitFor(cache)
    .WaitFor(rabbitMq)
    .WaitFor(keycloak);

var webapp = builder
    .AddNpmApp("webapp", "../../frontend/webapp")
    .WithEnvironment("BROWSER", "none")
    .WithHttpsEndpoint(env: "VITE_PORT")
    .WithExternalHttpEndpoints()
    .WithReference(catalog)
    .WithReference(basket)
    .WaitFor(basket)
    .WaitFor(catalog)
    .PublishAsDockerFile();

builder.Build().Run();
