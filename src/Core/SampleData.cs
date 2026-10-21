using Core.Domain;

namespace Core;

public static class SampleData
{
    public static List<Product> Products() =>
    [
        Product.Create("P-001", "SKU-001", "Цемент М400 25кг", "шт", 120),
        Product.Create("P-002", "SKU-002", "Пісок будівельний", "т", 18),
        Product.Create("P-003", "SKU-003", "Цегла червона", "шт", 4200),
        Product.Create("P-004", "SKU-004", "Фарба водоемульсійна 10л", "шт", 36),
        Product.Create("P-005", "SKU-005", "Шпаклівка фінішна", "кг", 250),
    ];
}
