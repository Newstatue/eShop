namespace Webhook;

sealed record WebhookOptions
{
    public const string SectionName = "Webhook";
    public string? Secret { get; init; }
    public string SignatureHeader { get; init; } = "X-PhaseTwo-Signature";
    public string? CallbackUrl { get; init; }
    public string[] EventTypes { get; init; } = ["*"];
}
