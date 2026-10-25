namespace Ch11.Ci.Production;

/// <summary>一次构建产出的一个产物:名字与字节数(真实流水线里对应包、二进制、迁移脚本等)。</summary>
public sealed record Artifact(string Name, long SizeBytes);

/// <summary>
/// 一次构建的产物清单。纯内存、无副作用——unit 层的理想被测对象;
/// 把它写到真实磁盘上的职责在 ArtifactStore(集成层)。
/// </summary>
public sealed class DeploymentInventory
{
    private readonly Dictionary<string, Artifact> artifacts = [];

    public IReadOnlyCollection<Artifact> Artifacts => artifacts.Values.ToList();

    public long TotalBytes => artifacts.Values.Sum(a => a.SizeBytes);

    /// <summary>登记产物;同名产物以最后一次为准(构建目录里重复输出是常见情形)。</summary>
    public void Add(Artifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        ArgumentException.ThrowIfNullOrWhiteSpace(artifact.Name);
        ArgumentOutOfRangeException.ThrowIfNegative(artifact.SizeBytes);
        artifacts[artifact.Name] = artifact;
    }

    public Artifact? Find(string name) =>
        artifacts.TryGetValue(name, out var artifact) ? artifact : null;
}
