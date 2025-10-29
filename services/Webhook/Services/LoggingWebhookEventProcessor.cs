using Webhook.Models;

namespace Webhook.Services;

sealed class LoggingWebhookEventProcessor(ILogger<LoggingWebhookEventProcessor> logger) : IWebhookEventProcessor
{
    public Task ProcessAsync(KeycloakWebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("收到 Keycloak Webhook：事件={EventType}，用户={UserId}，Realm={Realm}",
            webhookEvent.EventType, webhookEvent.UserId ?? "未知", webhookEvent.RealmId ?? "未知");

        logger.LogInformation("Webhook 原始载荷：{Payload}", webhookEvent.RawPayload);

        return Task.CompletedTask;
    }
}
