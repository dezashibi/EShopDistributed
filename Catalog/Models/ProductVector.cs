using Microsoft.Extensions.VectorData;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalog.Models;

public sealed class ProductVector
{
    [VectorStoreKey]
    public int Id { get; set; }

    [VectorStoreData]
    public string Name { get; set; } = default!;

    [VectorStoreData]
    public string Description { get; set; } = default!;

    [VectorStoreData]
    public decimal Price { get; set; }

    [VectorStoreData]
    public string ImageUrl { get; set; } = default!;

    // Keep this if ProductVector is also used as an EF entity somewhere.
    [NotMapped]
    [VectorStoreVector(Dimensions: 384, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}
