using Core.Domain;

namespace Core.Tests;

internal static class TestData
{
    public static List<Product> Items() =>
    [
        Product.Create("T-1", "SKU-BIG", "Великий", "шт", 100),
        Product.Create("T-2", "SKU-MID", "Середній", "шт", 50),
        Product.Create("T-3", "SKU-LOW1", "Малий 1", "шт", 5),
        Product.Create("T-4", "SKU-LOW2", "Малий 2", "шт", 9),
    ];
}
