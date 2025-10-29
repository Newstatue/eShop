namespace ServiceDefaults.Messaging.Events;

public sealed record ProductPriceChangedIntegrationEvent : IntegrationEvent
{
    public int ProductId { get; init; }
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public decimal Price { get; init; }
    public string ImageUrl { get; init; } = default!;
}
