using Xunit;

// 程序集级开关:整个程序集里的测试都会把 Console / Debug / Trace 的输出重定向进"测试输出",
// 效果等同于测试自己用 ITestOutputHelper 写的行。默认是关闭的,所以第 9 章把这两个特性
// 单独放进 Ch09.Output.Capture 这个工程——副作用只影响本工程,不影响其它章的工程。
[assembly: CaptureConsole]
[assembly: CaptureTrace]
