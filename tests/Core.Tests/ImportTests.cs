using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class ImportTests
{
    [Fact] // 5) скасування
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var service = new CatalogService(new InMemoryCatalogStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), "any.json", cts.Token));
    }

    [Fact] // 6) доменна помилка на зіпсованих даних (тимчасовий файл)
    public async Task LoadAsync_WithInvalidRecord_ThrowsDomainException()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(path,
                """[{"id":"X-1","sku":"","name":"Bad","unit":"шт","quantity":1}]""");

            var ex = await Assert.ThrowsAsync<DomainException>(
                () => new JsonCatalogImporter().LoadAsync(path));

            Assert.Contains("Запис 1", ex.Message);
        }
        finally { File.Delete(path); }
    }
}
