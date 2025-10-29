namespace Webhook.Options;

sealed record WebhookOptions
{
    public const string SectionName = "Webhook";
    public string? Secret { get; init; }
    public string SignatureHeader { get; init; } = "X-Keycloak-Signature";
    public string? CallbackUrl { get; init; }
    public string[] EventTypes { get; init; } = ["*"];
}
