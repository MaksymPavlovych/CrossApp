using Core.Reports;

namespace Core.Tests;

public class ReportServiceTests
{
    [Theory] // 3) звіт на кількох наборах даних
    [InlineData(10, 2)]
    [InlineData(1, 0)]
    public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected) =>
        Assert.Equal(expected, ReportService.LowStock(TestData.Items(), threshold).Count);

    [Fact] // 4) детермінований топ-N
    public void TopByQuantity_IsDeterministic() =>
        Assert.Equal(new[] { "SKU-BIG", "SKU-MID" },
            ReportService.TopByQuantity(TestData.Items(), 2).Select(l => l.Sku));
}
