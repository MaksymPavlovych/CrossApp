using System.Text.Json;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Reports;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

var positional = args.Where(a => !a.StartsWith("--")).ToArray();
var command = positional.FirstOrDefault() ?? "report-top";

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
if (args.Contains("--cancel")) cts.Cancel(); // для демонстрації скасування
// cts.CancelAfter(TimeSpan.FromSeconds(2));  // варіант: скасування за таймаутом

Console.WriteLine($"Сховище: {store.GetType().Name}");

try
{
    switch (command)
    {
        case "report-top":
            Print(ReportService.TopByQuantity(service.All(), 5));
            break;
        case "report-low":
            Print(ReportService.LowStock(service.All(), 10));
            break;
        case "report-groups":
            foreach (var g in ReportService.ByUnit(service.All()))
                Console.WriteLine($"  {g.Key,-6} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
            break;
        case "report-sum":
            var t = ReportService.Totals(service.All());
            Console.WriteLine($"  позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
            break;
        case "import":
        {
            var importPath = positional.Skip(1).FirstOrDefault() ?? Path.Combine("data", "import.json");
            var added = await service.ImportAsync(new JsonCatalogImporter(), importPath, cts.Token);
            Console.WriteLine($"Імпортовано записів: {added}");
            break;
        }
        default:
            Console.WriteLine("report-top | report-low | report-groups | report-sum | import [шлях]");
            break;
    }
}
catch (OperationCanceledException) { Console.WriteLine("Імпорт скасовано."); }
catch (DomainException ex) { Console.WriteLine($"Дані відхилено: {ex.Message}"); }
catch (FileNotFoundException ex) { Console.WriteLine($"Файл не знайдено: {ex.FileName}"); }
catch (JsonException ex) { Console.WriteLine($"Файл пошкоджений (JSON): {ex.Message}"); }

static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
        Console.WriteLine($"  {l.Sku,-10} {l.Name,-32} {l.Quantity,6} {l.Unit}");
}
