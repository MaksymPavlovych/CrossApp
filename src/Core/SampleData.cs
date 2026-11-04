using Core.Domain;

namespace Core;

public static class SampleData
{
    public static List<Product> Products() =>
    [
        Product.Create("P-001", "SKU-001", "Цемент М400 25кг", "шт", 120),
        Product.Create("P-002", "SKU-002", "Пісок будівельний", "т", 18),
        Product.Create("P-003", "SKU-003", "Цегла червона", "шт", 420),
        Product.Create("P-004", "SKU-004", "Фарба водоемульсійна 10л", "шт", 36),
        Product.Create("P-005", "SKU-005", "Шпаклівка фінішна", "кг", 250),
        Product.Create("P-006", "SKU-006", "Профіль CD-60", "м", 800),
        Product.Create("P-007", "SKU-007", "Саморізи 3.5x25", "уп", 140),
        Product.Create("P-008", "SKU-008", "Грунтовка глибокого проникнення", "л", 60),
        Product.Create("P-009", "SKU-009", "Плитка керамічна", "м2", 310),
        Product.Create("P-010", "SKU-010", "Клей плитковий", "кг", 540),
        Product.Create("P-011", "SKU-011", "Гіпсокартон 12.5мм", "шт", 85),
        Product.Create("P-012", "SKU-012", "Стрічка армуюча", "м", 300),
        Product.Create("P-013", "SKU-013", "Кабель UTP cat6", "м", 150),
        Product.Create("P-014", "SKU-014", "Дюбель 6x40", "уп", 200),
        Product.Create("P-015", "SKU-015", "Рукавички робочі", "шт", 7),
        Product.Create("P-016", "SKU-016", "Скоба будівельна", "уп", 4),
        Product.Create("P-017", "SKU-017", "Ізострічка", "шт", 9),
        Product.Create("P-018", "SKU-018", "Пігмент синій", "кг", 25),
    ];
}
