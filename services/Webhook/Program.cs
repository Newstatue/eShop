using System.Reflection;

using ServiceDefaults.Messaging;

using Webhook;
using Webhook.Endpoints;
using Webhook.Options;
using Webhook.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHealthChecks();
builder.Services.Configure<WebhookOptions>(builder.Configuration.GetSection(WebhookOptions.SectionName));
builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection(KeycloakOptions.SectionName));
builder.Services.AddHttpClient(nameof(KeycloakAdminTokenProvider));
builder.Services.AddSingleton<IKeycloakAdminTokenProvider, KeycloakAdminTokenProvider>();
builder.Services.AddHttpClient(KeycloakWebhookRegistrar.HttpClientName);
builder.Services.AddHostedService<KeycloakWebhookRegistrar>();

builder.Services.AddMassTransitWithAssemblies(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton<IWebhookEventProcessor, LoggingWebhookEventProcessor>();
builder.Services.AddSingleton<IWebhookEventProcessor, RabbitMQWebhookEventProcessor>();
builder.Services.AddSingleton<KeycloakWebhookEndpoint>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapPost("/webhook/keycloak",
    (KeycloakWebhookEndpoint endpoint, HttpRequest request, CancellationToken cancellationToken) =>
        endpoint.HandleAsync(request, cancellationToken));

await app.RunAsync();
