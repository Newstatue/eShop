using MassTransit;

using ServiceDefaults.Messaging.Events;

using Webhook.Models;

namespace Webhook.Services;

public class RabbitMQWebhookEventProcessor(IBus bus, ILogger<RabbitMQWebhookEventProcessor> logger)
    : IWebhookEventProcessor
{
    public async Task ProcessAsync(KeycloakWebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        // 1️⃣ 记录收到的 webhook 基本信息
        logger.LogInformation(
            "收到 Keycloak Webhook：事件={EventType}，用户={UserId}，Realm={RealmId}",
            webhookEvent.EventType,
            string.IsNullOrWhiteSpace(webhookEvent.UserId) ? "未知" : webhookEvent.UserId,
            webhookEvent.RealmId
        );

        // 2️⃣ 构建要发布的 Integration Event
        var integrationEvent = new KeycloakWebhookIntegrationEvent
        {
            EventType = webhookEvent.EventType,
            UserId = webhookEvent.UserId,
            RealmId = webhookEvent.RealmId,
            RawPayload = webhookEvent.RawPayload,
            Headers = new Dictionary<string, string>(webhookEvent.Headers, StringComparer.OrdinalIgnoreCase),
            Payload = webhookEvent.Payload
        };

        // 3️⃣ 发布事件到 RabbitMQ
        try
        {
            await bus.Publish(integrationEvent, cancellationToken);
            logger.LogInformation(
                "已发布 KeycloakWebhookIntegrationEvent 到总线：EventType={EventType}, Realm={RealmId}, UserId={UserId}",
                integrationEvent.EventType,
                integrationEvent.RealmId,
                integrationEvent.UserId
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "发布 KeycloakWebhookIntegrationEvent 时发生错误：EventType={EventType}, Realm={RealmId}, UserId={UserId}",
                integrationEvent.EventType,
                integrationEvent.RealmId,
                integrationEvent.UserId
            );
            throw;
        }
    }
}
