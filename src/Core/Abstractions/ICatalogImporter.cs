using Core.Domain;

namespace Core.Abstractions;

public interface ICatalogImporter
{
    Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default);
}
