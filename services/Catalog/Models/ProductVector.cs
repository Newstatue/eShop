using System.ComponentModel.DataAnnotations.Schema;

namespace Catalog.Models;

public class ProductVector
{
    [VectorStoreKey] public int Id { get; set; }
    [VectorStoreData] public string Name { get; set; } = null!;
    [VectorStoreData] public string Description { get; set; } = null!;
    [VectorStoreData] public string Brand { get; set; } = null!;
    [VectorStoreData] public decimal BasePrice { get; set; }
    [VectorStoreData] public string? PrimaryImageUrl { get; set; }

    [NotMapped]
    [VectorStoreVector(
        384,
        DistanceFunction =
            nameof(DistanceFunction.CosineSimilarity
            ))]
    public ReadOnlyMemory<float> Vector { get; set; }
}
