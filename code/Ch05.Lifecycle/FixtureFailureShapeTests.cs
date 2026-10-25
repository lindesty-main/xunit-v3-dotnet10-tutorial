// 5.8 失败形态演示用的程序集夹具:CH05_FAIL=assembly-setup 时构造抛错,
// 整个程序集的每个测试都以同一条消息失败;默认运行不受影响。
// (程序集级特性必须写在命名空间声明之前,所以用完全限定名。)
[assembly: AssemblyFixture<Ch05.Lifecycle.Tests.FragileAssemblyFixture>]

namespace Ch05.Lifecycle.Tests;

public class FragileAssemblyFixture
{
    public FragileAssemblyFixture()
    {
        if (Environment.GetEnvironmentVariable("CH05_FAIL") == "assembly-setup")
            throw new InvalidOperationException("模拟:依赖容器启动失败");
    }
}

// 程序集夹具的构造签名在编译期就被分析器检查(xUnit3005)。若写:
//
//   public class BadAssemblyCtor(int x) { }
//   [assembly: AssemblyFixture<BadAssemblyCtor>]
//
// 编译直接停下(xUnit3005),真实错误文本见文档 5.5。

/// <summary>
/// 夹具失败的报告形态(文档 5.8)。默认全绿;两种注入失败的方式:
/// CH05_FAIL=setup   → 夹具构造抛错,失败记在测试头上;
/// CH05_FAIL=dispose → 夹具 Dispose 抛错,失败是独立的 Cleanup Failure 条目,测试本身仍是 Pass。
/// </summary>
public sealed class FragileFixture : IDisposable
{
    public FragileFixture()
    {
        if (Environment.GetEnvironmentVariable("CH05_FAIL") == "setup")
            throw new InvalidOperationException("模拟:外部依赖初始化失败");
    }

    public void Dispose()
    {
        if (Environment.GetEnvironmentVariable("CH05_FAIL") == "dispose")
            throw new InvalidOperationException("模拟:资源清理失败");
    }
}

public sealed class FixtureFailureShapeDemoTests : IClassFixture<FragileFixture>
{
    [Fact]
    public void Demo_Passes_WhenNoFailureEnvIsSet()
    {
        // 这条测试只证明"默认情况下夹具是健康的";
        // 两种失败形态的真实输出来自文档 5.8 里带 CH05_FAIL 的两条命令
        Assert.True(true);
    }
}
