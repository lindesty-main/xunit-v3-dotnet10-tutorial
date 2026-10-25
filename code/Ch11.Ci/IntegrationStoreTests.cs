using Ch11.Ci.Production;

namespace Ch11.Ci.Tests;

/// <summary>
/// integration 层:真的写磁盘、读回来逐字段比对。
/// 前提(一个可写目录)由环境提供:CI 的集成 job 设置 CH11_STORE_DIR,
/// 本地默认没有 → 动态跳过,原因里写清"怎么让它跑起来"(第 7 章的写法)。
/// </summary>
[Trait("kind", "integration")]
public class IntegrationStoreTests
{
    private const string SkipReason =
        "集成层未配置:设置 CH11_STORE_DIR=<可写目录> 后再运行这一层(CI 的集成 job 会自动设置)";

    [Fact]
    public void Store_SaveThenLoad_RoundTripsAllArtifacts()
    {
        Assert.SkipUnless(RunEnvironment.StoreDirectory is not null, SkipReason);

        var store = NewStoreFor(nameof(Store_SaveThenLoad_RoundTripsAllArtifacts));
        var inventory = new DeploymentInventory();
        inventory.Add(new Artifact("app.dll", 1_000));
        inventory.Add(new Artifact("发布 说明.txt", 37));
        store.Save(inventory);

        var loaded = store.Load();

        Assert.Equal(1_037, loaded.TotalBytes);
        Assert.NotNull(loaded.Find("app.dll"));
        Assert.NotNull(loaded.Find("发布 说明.txt"));
    }

    [Fact]
    public void Store_ManifestIsPlainJson_OnDisk()
    {
        Assert.SkipUnless(RunEnvironment.StoreDirectory is not null, SkipReason);

        var store = NewStoreFor(nameof(Store_ManifestIsPlainJson_OnDisk));
        var inventory = new DeploymentInventory();
        inventory.Add(new Artifact("app.dll", 1_000));
        store.Save(inventory);

        // CI 的 upload-artifact 收集的就是这个文件:它必须是可读的 JSON,不是二进制
        var json = File.ReadAllText(store.ManifestFilePath);
        Assert.Contains("\"app.dll\"", json);
        Assert.Contains("1000", json);
    }

    [Fact]
    public void Store_SecondSave_ReplacesManifest()
    {
        Assert.SkipUnless(RunEnvironment.StoreDirectory is not null, SkipReason);

        var store = NewStoreFor(nameof(Store_SecondSave_ReplacesManifest));
        var first = new DeploymentInventory();
        first.Add(new Artifact("old.dll", 1));
        store.Save(first);

        var second = new DeploymentInventory();
        second.Add(new Artifact("new.dll", 2));
        store.Save(second);

        var loaded = store.Load();
        Assert.Null(loaded.Find("old.dll"));
        Assert.Equal(2, loaded.Find("new.dll")!.SizeBytes);
    }

    private static ArtifactStore NewStoreFor(string caseName)
    {
        // 每条用例自己的子目录:集成测试之间也不共享状态(第 5 章的原则,到磁盘上同样成立)
        var directory = Path.Combine(RunEnvironment.StoreDirectory!, caseName);
        if (Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
        return new ArtifactStore(directory);
    }
}
