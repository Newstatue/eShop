namespace ServiceDefaults.Messaging.Events;

public sealed record ProductCreatedIntegrationEvent : IntegrationEvent
{
    public int ProductId { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string Brand { get; init; } = null!;
    public decimal BasePrice { get; init; }
    public bool UseAIGeneratedRichDescription { get; init; }
}
