using Ch11.Ci.Production;

namespace Ch11.Ci.Tests;

/// <summary>
/// unit 层:被测对象全是纯内存逻辑,不碰磁盘、网络、环境变量。
/// CI 里每个 job 都会跑这一层(11.3),所以它必须快且在任何环境结果一致。
/// </summary>
[Trait("kind", "unit")]
public class UnitDeploymentTests
{
    [Fact]
    public void Inventory_TotalBytes_SumsAllArtifacts()
    {
        var inventory = new DeploymentInventory();
        inventory.Add(new Artifact("app.dll", 1_000));
        inventory.Add(new Artifact("migrate.sql", 250));

        Assert.Equal(1_250, inventory.TotalBytes);
    }

    [Fact]
    public void Inventory_DuplicateName_KeepsLatest()
    {
        var inventory = new DeploymentInventory();
        inventory.Add(new Artifact("app.dll", 1_000));
        inventory.Add(new Artifact("app.dll", 2_500));

        Assert.Equal(2_500, inventory.Find("app.dll")!.SizeBytes);
        Assert.Single(inventory.Artifacts);
        Assert.Equal(2_500, inventory.TotalBytes);
    }

    [Fact]
    public void Inventory_NegativeSize_IsRejected()
    {
        var inventory = new DeploymentInventory();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => inventory.Add(new Artifact("app.dll", -1)));
    }

    [Fact]
    public void Gate_SkipsAreFine_WhenFailSkipsIsOff()
    {
        var gate = new GatePolicy(failSkips: false, maxTotalBytes: 10_000);

        var decision = gate.Evaluate(new RunCounts(Succeeded: 10, Failed: 0, Skipped: 3), new DeploymentInventory());

        Assert.True(decision.Passed);
    }

    [Fact]
    public void Gate_SkipsFailTheRun_WhenFailSkipsIsOn()
    {
        var gate = new GatePolicy(failSkips: true, maxTotalBytes: 10_000);

        var decision = gate.Evaluate(new RunCounts(Succeeded: 10, Failed: 0, Skipped: 1), new DeploymentInventory());

        Assert.False(decision.Passed);
        Assert.Contains("skipped", decision.Reason);
    }

    [Fact]
    public void Gate_AnyFailedTest_FailsTheRun()
    {
        var gate = new GatePolicy(failSkips: false, maxTotalBytes: 10_000);

        var decision = gate.Evaluate(new RunCounts(Succeeded: 10, Failed: 1, Skipped: 0), new DeploymentInventory());

        Assert.False(decision.Passed);
        Assert.Contains("failed", decision.Reason);
    }

    [Fact]
    public void Gate_OversizedArtifacts_FailTheRun()
    {
        var gate = new GatePolicy(failSkips: false, maxTotalBytes: 1_000);
        var inventory = new DeploymentInventory();
        inventory.Add(new Artifact("bundle.zip", 1_001));

        var decision = gate.Evaluate(new RunCounts(Succeeded: 10, Failed: 0, Skipped: 0), inventory);

        Assert.False(decision.Passed);
        Assert.Contains("exceeds limit", decision.Reason);
    }
}
