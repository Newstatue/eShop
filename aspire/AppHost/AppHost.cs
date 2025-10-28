var builder = DistributedApplication.CreateBuilder(args);

//参数
var keycloakAdminUser = builder
    .AddParameter("keycloak-admin-user");
var keycloakAdminPassword = builder
    .AddParameter("keycloak-admin-password", secret: true)
    .WithGeneratedDefault(new() { MinLength = 12, Special = true });

var keycloakRealmName = builder
    .AddParameter("eshop-realm");
var keycloakRealmDisplayName = builder
    .AddParameter("eshop-realm-display");

var webAppClientId = builder
    .AddParameter("webapp-client-id");
var webAppClientName = builder
    .AddParameter("webapp-client-name");
// var webAppClientSecret = builder
//     .AddParameter("webapp-client-secret", secret: true)
//     .WithGeneratedDefault(new() { MinLength = 32, Special = false });

var testUserUsername = builder
    .AddParameter("test-user-username");
var testUserEmail = builder
    .AddParameter("test-user-email");
var testUserPassword = builder
    .AddParameter("test-user-password", secret: true);


//微服务
var postgres = builder
    .AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var catalogDb = postgres
    .AddDatabase("catalogdb");

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
    .WithImage("phasetwo/phasetwo-keycloak")
    .WithImageTag("25")
    .WithBindMount("./providers", "/opt/keycloak/providers")
    // .WithDataVolume()
    .RunWithHttpsDevCertificate()
    .WithEnvironment("KEYCLOAK_ADMIN", keycloakAdminUser)
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", keycloakAdminPassword)
    .WithLifetime(ContainerLifetime.Persistent);

var managementHealthChecks = keycloak.Resource.Annotations
    .OfType<HealthCheckAnnotation>()
    .Where(hc => hc.Key.Contains("management", StringComparison.OrdinalIgnoreCase))
    .ToList();

foreach (var annotation in managementHealthChecks)
{
    keycloak.Resource.Annotations.Remove(annotation);
}

keycloak.WithHttpHealthCheck();

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

var webApp = builder
    .AddNpmApp("webapp", "../../frontend/webapp")
    .WithEnvironment("BROWSER", "none")
    .WithHttpsEndpoint(env: "VITE_PORT")
    .WithExternalHttpEndpoints()
    .WithReference(catalog)
    .WithReference(basket)
    .WithReference(keycloak)
    .WaitFor(basket)
    .WaitFor(catalog)
    .WaitFor(keycloak)
    .WithEnvironment("Authentication__Keycloak__Realm", keycloakRealmName)
    .WithEnvironment("Authentication__Schemes__OpenIdConnect__ClientId", webAppClientId)
    // .WithEnvironment("Authentication__Schemes__OpenIdConnect__ClientSecret", webAppClientSecret)
    .PublishAsDockerFile();

keycloak.WithSampleRealmImport(keycloakRealmName, keycloakRealmDisplayName, [
    new KeycloakClientDetails(
        "WEB_APP", // 用于 Realm JSON 的环境变量前缀
        webAppClientId,
        webAppClientName,
        null,
        webApp
    )
]);
keycloak
    .WithEnvironment("TEST_USER_USERNAME", testUserUsername)
    .WithEnvironment("TEST_USER_EMAIL", testUserEmail)
    .WithEnvironment("TEST_USER_PASSWORD", testUserPassword)
    .WithEnvironment("TEST_USER_LOCALE", "zh-CN")
    .WithEnvironment("TEST_USER_CREATED_TIMESTAMP", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());

var apiPortal = builder
    .AddProject<Projects.ApiPortal>("apiportal")
    .WithHttpsEndpoint()
    .WithReference(catalog)
    .WithReference(basket)
    .WithExternalHttpEndpoints()
    .WaitFor(catalog)
    .WaitFor(basket);

apiPortal
    .WithEnvironment("Keycloak__Realm", keycloakRealmName)
    .WithEnvironment("Keycloak__ClientId", webAppClientId)
    .WithEnvironment("Keycloak__DefaultUser__Username", testUserUsername)
    .WithEnvironment("Keycloak__DefaultUser__Password", testUserPassword);

apiPortal.WithEnvironment(context =>
{
    var httpsEndpoint = keycloak.GetEndpoint("https");
    var authority = httpsEndpoint.Url;
    context.EnvironmentVariables["Keycloak__Authority"] = authority.TrimEnd('/');
});


builder.Build().Run();
