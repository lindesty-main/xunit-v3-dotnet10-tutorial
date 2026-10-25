namespace Ch01.GettingStarted.Tests;

/// <summary>
/// 分析器演示:本文件故意保留了 v3 不支持的 async void 写法,
/// 并用 #pragma 与 Skip 把它压住,好让本章工程仍然能编译、仍然全绿。
/// 正常项目里应当直接采用下面修复后的写法。
/// </summary>
public class AnalyzerDemoTests
{
    // 演示对象:分析器会在这里报 xUnit1049(错误级)。
    // 因为 xUnit1049 会直接让编译失败,这里临时压住它,只为把这条诊断展示出来。
#pragma warning disable xUnit1049 // 演示用:故意保留 v3 已移除的 async void 测试
    [Fact(Skip = "演示用:async void 在 v3 会被运行时快速失败,修复见下一个测试")]
    public async void AsyncVoidTest()
    {
        await Task.Yield();
        Assert.True(true);
    }
#pragma warning restore xUnit1049

    // 修复后的写法:async Task(或 async ValueTask)
    [Fact]
    public async Task AsyncVoidTest_Fixed()
    {
        await Task.Yield();
        Assert.True(true);
    }
}
