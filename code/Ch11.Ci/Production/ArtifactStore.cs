using System.Text.Json;

namespace Ch11.Ci.Production;

/// <summary>
/// 把产物清单写到真实磁盘目录(模拟 CI 的产物仓库/artifact store)。
/// 目录从哪来、不存在怎么办都是环境的事——所以它属于集成层,不属于 unit 层。
/// </summary>
public sealed class ArtifactStore
{
    private readonly string directory;

    public ArtifactStore(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        this.directory = directory;
    }

    public string ManifestFilePath => Path.Combine(directory, "inventory.json");

    public void Save(DeploymentInventory inventory)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(ManifestFilePath, JsonSerializer.Serialize(inventory.Artifacts));
    }

    public DeploymentInventory Load()
    {
        var items = JsonSerializer.Deserialize<List<Artifact>>(File.ReadAllText(ManifestFilePath))
            ?? throw new InvalidDataException($"empty or invalid manifest: {ManifestFilePath}");
        var inventory = new DeploymentInventory();
        foreach (var item in items)
            inventory.Add(item);
        return inventory;
    }
}
