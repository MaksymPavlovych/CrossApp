using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Domain;

namespace Core.Storage;

public static class CatalogJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // кирилиця читабельна, а не \u0426
    };

    // Атомарний запис: спочатку .tmp, потім миттєва підміна
    public static async Task<int> ExportAsync(IEnumerable<Product> items, string path,
        CancellationToken ct = default)
    {
        var dtos = items.Select(p => p.ToDto()).ToList();
        var full = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);

        var tmp = full + ".tmp";
        await File.WriteAllTextAsync(tmp, JsonSerializer.Serialize(dtos, Options), ct);
        File.Move(tmp, full, overwrite: true);
        return dtos.Count;
    }
}
