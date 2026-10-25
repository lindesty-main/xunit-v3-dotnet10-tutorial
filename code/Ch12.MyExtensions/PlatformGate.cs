using System.Runtime.InteropServices;

namespace Ch12.MyExtensions;

/// <summary>
/// 平台门槛属性:供 FactAttribute 的 SkipUnless/SkipWhen + SkipType 引用的
/// public static bool 属性。放在库里,多个测试工程都能复用同一套门槛。
/// </summary>
public static class PlatformGate
{
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
}
