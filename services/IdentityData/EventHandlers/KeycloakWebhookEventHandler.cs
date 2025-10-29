using IdentityData.Services;

using ServiceDefaults.Messaging.Events;

namespace IdentityData.EventHandlers;

/// <summary>
/// Keycloak Webhook 事件分发器
/// 将不同类型的 Keycloak 事件路由到对应的业务处理方法。
/// </summary>
public sealed class KeycloakWebhookEventHandler(
    IUserEventHandler userEventHandler,
    ILogger<KeycloakWebhookEventHandler> logger)
    : IConsumer<KeycloakWebhookIntegrationEvent>
{
    public async Task Consume(ConsumeContext<KeycloakWebhookIntegrationEvent> context)
    {
        var evt = context.Message;
        var type = evt.EventType.Trim().ToLowerInvariant();

        switch (type)
        {
            // 注册事件
            case "REGISTER":
            case "access.REGISTER":
                await userEventHandler.HandleRegisterAsync(evt);
                break;

            // 登录事件
            case "LOGIN":
            case "access.LOGIN":
                await userEventHandler.HandleLoginAsync(evt);
                break;

            // 其他事件忽略
            default:
                logger.LogDebug("忽略未处理的 Keycloak 事件类型: {EventType}", evt.EventType);
                break;
        }
    }
}
