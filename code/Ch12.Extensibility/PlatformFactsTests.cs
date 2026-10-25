using Ch12.MyExtensions;

namespace Ch12.Extensibility.Tests;

/// <summary>
/// 自定义 Fact 派生类:平台门槛封装在特性构造函数里(Skip + SkipUnless + SkipType),
/// 使用处只剩 [LinuxFact] / [WindowsFact]。
/// </summary>
public class PlatformFactsTests
{
    [LinuxFact]
    public void ProcSelfStatus_HasNameField()
    {
        var status = File.ReadAllText("/proc/self/status");
        Assert.Contains("Name:", status);
    }

    // 本机是 Linux:这条会在运行时被动态跳过,原因取 Skip 的消息。
    [WindowsFact]
    public void WindowsFolder_IsResolvable()
    {
        var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        Assert.False(string.IsNullOrEmpty(windir));
    }
}
