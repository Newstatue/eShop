using Webhook.Models;

namespace Webhook.Services;

sealed class LoggingWebhookEventProcessor(ILogger<LoggingWebhookEventProcessor> logger) : IWebhookEventProcessor
{
    public Task ProcessAsync(KeycloakWebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("收到 Keycloak Webhook 数据：{Payload}", webhookEvent);

        return Task.CompletedTask;
    }
}
