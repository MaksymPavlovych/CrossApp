using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryCatalogStore(IEnumerable<Product>? seed = null) : ICatalogStore
{
    private readonly Dictionary<string, Product> _items =
        (seed ?? []).ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Product> List() => _items.Values.ToList();
    public Product? GetById(string id) => _items.GetValueOrDefault(id);
    public void Update(Product item) => _items[item.Id] = item;
    public bool Remove(string id) => _items.Remove(id);

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");
        _items.Add(item.Id, item);
    }
}
