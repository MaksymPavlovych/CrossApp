using Core;
using Core.Abstractions;
using Core.Events;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");

// --- Підписник 1: локальний метод (його можна буде відписати) ---
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} → залишок {e.QuantityAfter}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

service.Changed += OnCatalogChanged;

// --- Підписник 2: лямбда із замиканням над змінною changes ---
int changes = 0;
service.Changed += (_, _) => changes++;

// --- Сценарій успіху ---
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
service.Receive(created.Id, 25);
Console.WriteLine($"Подій отримано: {changes}");

// --- Відписка: операція виконується, повідомлення в консолі/лозі немає ---
service.Changed -= OnCatalogChanged;
service.Receive(created.Id, 5);
Console.WriteLine($"Подій отримано: {changes}"); // +1 лише від лямбди-лічильника

// --- Сценарій відмови: виняток є, події немає ---
try
{
    service.Receive(created.Id, -10);
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}

try
{
    service.Receive("NON-EXISTENT-ID", 10);
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}

// --- Func замість події: пошук за критерієм ---
service.Add("SKU-102", "Ізострічка", "шт", 5);
Console.WriteLine("Залишок менше 10:");
foreach (var p in service.FindAll(p => p.Quantity < 10))
    Console.WriteLine($"  {p.Sku} {p.Name} — {p.Quantity} {p.Unit}");
