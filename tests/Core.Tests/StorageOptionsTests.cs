using Core.Configuration;

namespace Core.Tests;

public class StorageOptionsTests
{
    [Fact]
    public void ResolveFullPath_RelativePath_UsesBaseDirectory()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var options = new StorageOptions { DataPath = "data", FileName = "items.json" };

            var full = options.ResolveFullPath(baseDir);

            Assert.Equal(Path.GetFullPath(Path.Combine(baseDir, "data", "items.json")), full);
            Assert.True(Directory.Exists(Path.Combine(baseDir, "data")));
        }
        finally { if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true); }
    }

    [Fact]
    public void ResolveFullPath_AbsolutePath_IgnoresBaseDirectory()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var absolute = Path.Combine(baseDir, "abs");
        try
        {
            var options = new StorageOptions { DataPath = absolute, FileName = "items.json" };

            var full = options.ResolveFullPath("ІГНОРУЄТЬСЯ");

            Assert.Equal(Path.GetFullPath(Path.Combine(absolute, "items.json")), full);
        }
        finally { if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true); }
    }
}
