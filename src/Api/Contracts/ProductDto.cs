using Core.Domain;

namespace Api.Contracts;

public sealed record ProductDto(string Id, string Sku, string Name, string Unit, int Quantity)
{
    public static ProductDto From(Product p) => new(p.Id, p.Sku, p.Name, p.Unit, p.Quantity);
}
