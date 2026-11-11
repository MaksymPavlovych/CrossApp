using System.Reflection;
using System.Text.Json;
using Core;
using Core.Abstractions;
using Core.Configuration;
using Core.Domain;
using Core.Reports;
using Core.Services;
using Core.Storage;
using Microsoft.Extensions.Configuration;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ---------- Конфігурація: пізніші джерела перекривають попередні ----------
var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables(prefix: "CROSSAPP_")
    .AddCommandLine(args)
    .Build();

var options = config.Get<AppOptions>() ?? new AppOptions();

// проста змінна CROSSAPP_DATA_PATH перекриває розділ Storage
var legacy = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
var storage = new StorageOptions
{
    DataPath = string.IsNullOrWhiteSpace(legacy) ? options.Storage.DataPath : legacy,
    FileName = options.Storage.FileName
};

var dataFile = storage.ResolveFullPath(AppContext.BaseDirectory);
var apiKey = config["Integration:ApiKey"];

Console.WriteLine($"Середовище : {options.EnvironmentName} (лог: {options.Logging.MinimumLevel})");
Console.WriteLine($"Файл даних : {dataFile}");
Console.WriteLine(apiKey is null
    ? "ApiKey     : не заданий"
    : $"ApiKey     : присутній, довжина {apiKey.Length}"); // сам ключ НЕ друкуємо

// ---------- Композиція (composition root) ----------
bool useFile = args.Contains("--file");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataFile)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

var positional = args.Where(a => !a.StartsWith("--")).ToArray();
var command = positional.FirstOrDefault() ?? "report-top";
var pathArg = positional.Skip(1).FirstOrDefault();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
if (args.Contains("--cancel")) cts.Cancel();

Console.WriteLine($"Сховище    : {store.GetType().Name}");

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
        case "export":
        {
            var target = pathArg ?? dataFile;
            var count = await CatalogJson.ExportAsync(service.All(), target, cts.Token);
            Console.WriteLine($"Експортовано {count} записів.");
            break;
        }
        case "import":
        {
            var source = pathArg ?? dataFile;
            var added = await service.ImportAsync(new JsonCatalogImporter(), source, cts.Token);
            Console.WriteLine($"Імпортовано записів: {added}");
            break;
        }
        default:
            Console.WriteLine("report-top | report-low | report-groups | report-sum | export [шлях] | import [шлях]");
            break;
    }
}
catch (OperationCanceledException) { Console.WriteLine("Операцію скасовано."); }
catch (DomainException ex) { Console.WriteLine($"Дані відхилено: {ex.Message}"); }
catch (FileNotFoundException ex) { Console.WriteLine($"Файл не знайдено: {ex.FileName}"); }
catch (JsonException ex) { Console.WriteLine($"Файл пошкоджений (JSON): {ex.Message}"); }

static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
        Console.WriteLine($"  {l.Sku,-10} {l.Name,-32} {l.Quantity,6} {l.Unit}");
}
