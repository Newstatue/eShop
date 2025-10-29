using Webhook;
using Webhook.Endpoints;
using Webhook.Options;
using Webhook.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.Configure<WebhookOptions>(builder.Configuration.GetSection(WebhookOptions.SectionName));
builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection(KeycloakOptions.SectionName));
builder.Services.AddHttpClient(nameof(KeycloakAdminTokenProvider));
builder.Services.AddSingleton<IKeycloakAdminTokenProvider, KeycloakAdminTokenProvider>();
builder.Services.AddHttpClient(KeycloakWebhookRegistrar.HttpClientName);
builder.Services.AddHostedService<KeycloakWebhookRegistrar>();

builder.Services.AddSingleton<IWebhookEventProcessor, LoggingWebhookEventProcessor>();
builder.Services.AddSingleton<KeycloakWebhookEndpoint>();

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapPost("/webhook/keycloak",
    (KeycloakWebhookEndpoint endpoint, HttpRequest request, CancellationToken cancellationToken) =>
        endpoint.HandleAsync(request, cancellationToken));

await app.RunAsync();
