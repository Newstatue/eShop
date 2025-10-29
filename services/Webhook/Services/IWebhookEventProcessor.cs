using Webhook.Models;

namespace Webhook.Services;

public interface IWebhookEventProcessor
{
    Task ProcessAsync(KeycloakWebhookEvent webhookEvent, CancellationToken cancellationToken = default);
}
