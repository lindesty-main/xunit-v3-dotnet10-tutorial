using Ch11.Ci.Production;

namespace Ch11.Ci.Tests;

/// <summary>
/// smoke 层:不验证行为细节,只确认"构建产物的形状没变、默认门禁不会误伤"。
/// 每条都在毫秒级——smoke job 的价值在于快,任何让它变慢的断言都属于 unit 层。
/// </summary>
[Trait("kind", "smoke")]
public class SmokeDeploymentTests
{
    [Fact]
    public void Smoke_BuildOutput_HasExpectedShape()
    {
        var inventory = BuildSampleInventory();

        Assert.NotNull(inventory.Find("app.dll"));
        Assert.NotNull(inventory.Find("migrate.sql"));
        Assert.Equal(1_250, inventory.TotalBytes);
    }

    [Fact]
    public void Smoke_DefaultGate_AcceptsCleanRun()
    {
        var gate = GatePolicy.FromEnvironment();
        var inventory = BuildSampleInventory();

        var decision = gate.Evaluate(new RunCounts(Succeeded: 1, Failed: 0, Skipped: 0), inventory);

        Assert.True(decision.Passed);
    }

    private static DeploymentInventory BuildSampleInventory()
    {
        var inventory = new DeploymentInventory();
        inventory.Add(new Artifact("app.dll", 1_000));
        inventory.Add(new Artifact("migrate.sql", 250));
        return inventory;
    }
}
