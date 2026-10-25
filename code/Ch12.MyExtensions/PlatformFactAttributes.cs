namespace Ch12.MyExtensions;

// 派生 FactAttribute,把"只在某平台执行"的门槛封装进构造函数:
// 使用方写 [LinuxFact] / [WindowsFact] 即可,不必每次重复 Skip + SkipUnless + SkipType。
// 命名参数仍可在特性处覆盖,例如 [LinuxFact(Skip = "自定义跳过消息")].

/// <summary>只在 Linux 上执行的 Fact;其他平台运行时动态跳过。</summary>
public sealed class LinuxFactAttribute : FactAttribute
{
    public LinuxFactAttribute()
    {
        Skip = "此测试只在 Linux 上执行";
        SkipUnless = nameof(PlatformGate.IsLinux);
        SkipType = typeof(PlatformGate);
    }
}

/// <summary>只在 Windows 上执行的 Fact;其他平台运行时动态跳过。</summary>
public sealed class WindowsFactAttribute : FactAttribute
{
    public WindowsFactAttribute()
    {
        Skip = "此测试只在 Windows 上执行";
        SkipUnless = nameof(PlatformGate.IsWindows);
        SkipType = typeof(PlatformGate);
    }
}
