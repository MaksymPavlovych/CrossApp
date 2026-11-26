using System.Collections.Concurrent;
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryCatalogStore(IEnumerable<Product>? seed = null) : ICatalogStore
{
    private readonly ConcurrentDictionary<string, Product> _items = new(
        (seed ?? []).Select(p => KeyValuePair.Create(p.Id, p)),
        StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Product> List() => _items.Values.ToList();

    public Product? GetById(string id) => _items.TryGetValue(id, out var p) ? p : null;

    public void Update(Product item) => _items[item.Id] = item;

    public bool Remove(string id) => _items.TryRemove(id, out _);

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!_items.TryAdd(item.Id, item))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");
    }
}
