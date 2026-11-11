using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class CatalogService(ICatalogStore store)
{
    private readonly ICatalogStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public event EventHandler<CatalogChangedEventArgs>? Changed;

    private void OnChanged(Product p, CatalogChangeKind kind, int delta) =>
        Changed?.Invoke(this, new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity));

    public Product Add(string sku, string name, string unit, int quantity)
    {
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        _store.Add(product);
        OnChanged(product, CatalogChangeKind.Added, quantity); // після зміни, не до
        return product;
    }

    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id)
           ?? throw new DomainException($"Немає запису з id={id}.");
        product.RegisterArrival(quantity); // якщо тут виняток — події не буде
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    public void Issue(string id, int quantity)
    {
        var product = _store.GetById(id)
       ?? throw new DomainException($"Немає запису з id={id}.");
        product.Issue(quantity);
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Issued, -quantity);
    }

    public IReadOnlyList<Product> All() => _store.List();
    public Product? Find(string id) => _store.GetById(id);
    public async Task<int> ImportAsync(ICatalogImporter importer, string path,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var items = await importer.LoadAsync(path, ct);
        var added = 0;
        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();
            if (_store.GetById(item.Id) is not null) continue; // ідемпотентність: дублі пропускаємо
            _store.Add(item);
            OnChanged(item, CatalogChangeKind.Added, item.Quantity);
            added++;
        }
        return added;
    }
    // Критерій пошуку задає той, хто викликає
    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate) =>
        _store.List().Where(predicate).ToList();
}
