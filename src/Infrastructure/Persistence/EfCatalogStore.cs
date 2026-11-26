using Core;
using Core.Abstractions;
using Core.Domain;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public sealed class EfCatalogStore(AppDbContext db) : ICatalogStore
{
    // Рядок БД -> доменна сутність ТІЛЬКИ через фабрику (інваріанти перевіряються)
    private static Product ToDomain(ProductRow row) =>
        Product.Create(row.Id, row.Sku, row.Name, row.Unit, row.Quantity);

    private static ProductRow ToRow(Product p) => new()
    {
        Id = p.Id, Sku = p.Sku, Name = p.Name, Unit = p.Unit, Quantity = p.Quantity
    };

    public IReadOnlyList<Product> List() =>
        db.Products.AsNoTracking().OrderBy(x => x.Sku)
            .AsEnumerable().Select(ToDomain).ToList();

    public Product? GetById(string id) =>
        db.Products.AsNoTracking().SingleOrDefault(x => x.Id == id) is { } row
            ? ToDomain(row) : null;

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (db.Products.Any(x => x.Id == item.Id))
            throw new DomainException($"Запис з id={item.Id} уже існує.");
        if (db.Products.Any(x => x.Sku == item.Sku))
            throw new DomainException($"Артикул {item.Sku} уже існує.");

        db.Products.Add(ToRow(item));
        db.SaveChanges();
    }

    public void Update(Product item)
    {
        if (db.Products.Any(x => x.Sku == item.Sku && x.Id != item.Id))
            throw new DomainException($"Артикул {item.Sku} уже використовується іншим товаром.");

        var row = db.Products.Single(x => x.Id == item.Id);
        row.Sku = item.Sku;
        row.Name = item.Name;
        row.Unit = item.Unit;
        row.Quantity = item.Quantity;
        db.SaveChanges();
    }

    public bool Remove(string id)
    {
        var row = db.Products.SingleOrDefault(x => x.Id == id);
        if (row is null) return false;
        db.Products.Remove(row);
        db.SaveChanges();
        return true;
    }
}
