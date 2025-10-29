using ServiceDefaults.Messaging.Events;

namespace IdentityData.Services;

public interface IUserEventHandler
{
    Task HandleRegisterAsync(KeycloakWebhookIntegrationEvent evt);
    Task HandleLoginAsync(KeycloakWebhookIntegrationEvent evt);
}
