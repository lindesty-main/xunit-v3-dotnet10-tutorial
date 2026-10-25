# 第 1 章 · 认识 xUnit.net 与第一个测试

> 你将学会:
>
> - 判断该用哪个 xUnit.net 版本、该引用哪个 NuGet 包
> - 用官方模板创建 v3 测试工程,并逐行看懂它生成的每一个文件
> - 理解"v3 测试工程是一个独立可执行程序"这句话到底意味着什么
> - 写一个通过的测试和一个故意失败的测试,并读懂失败输出里的 Expected / Actual / 文件行号
> - 用 `dotnet test`、`dotnet run`、直接执行构建产物三种方式跑测试,并正确地把命令行选项传进去
> - 让 xUnit.net 分析器在编译期就把常见错误拦下来

本章的工程是 [`code/Ch01.GettingStarted`](../code/Ch01.GettingStarted)。所有输出片段都来自本机 .NET SDK 10.0.113 + xUnit.net v3 4.0.2 的真实运行;片段里过长的绝对路径用 `...` 省略,其余逐字保留。

## 学习起点与课程边界

开始前只需要会写简单的 C# 方法和类,并安装 .NET SDK;不要求有测试经验。Visual Studio、VS Code、Rider 或命令行都可以,选自己日常使用的工具即可。若部署后总担心改动会破坏功能、常要花很久定位 bug,或每次改动都要手工回归,本教程从可重复运行的测试开始解决这些问题。

本教程聚焦 **xUnit.net 框架及其工程用法**,不系统讲解单元测试方法论。路线从建项目、写基础测试和断言,逐步到参数化测试、夹具、执行控制、报告、CI 与扩展;章节以实际代码和练习为主。

**版本说明。** 本教程统一使用 xUnit.net v3 `4.0.2` 与稳定版 .NET 10 SDK,测试工程选用集成 MTP v2 的 `xunit.v3.mtp-v2` 包。`4.0.2` 是 v3 包的版本号,不表示这里切换到了 xUnit.net v4。

## 1.1 xUnit.net 是什么,该选哪个版本

xUnit.net 是一个 .NET 单元测试框架:它提供断言库、测试发现与执行引擎,以及命令行与 IDE 集成。MSTest 与 NUnit 扮演相同的总体角色,但它们的 API、生命周期和执行模型等细节各有差异;本教程只讲 xUnit.net。

`xUnit` 也常指跨语言测试框架遵循的一组模式;`xUnit.net` 是这组模式在 .NET 上的实现。测试类不必继承框架基类,测试由属性与约定发现;构造函数/`Dispose`、默认并行和扩展点等设计会在后续章节展开。

当前有两条产品线,选哪一条取决于你要不要新特性:

| 产品线 | 当前版本 | 状态 |
| --- | --- | --- |
| **v3(Core Framework)** | 4.0.2 | 主线,新工程应当用它 |
| v2 | 2.9.3 | 仅维护 |

v3 支持的最低运行时是 .NET 8 与 .NET Framework 4.7.2;本教程统一使用 `net10.0`。

### 包族怎么选

包名里的 MTP 后缀表示它与 Microsoft Testing Platform(MTP,微软新一代测试平台)的集成方式;不带后缀的 `xunit.v3` 就取当时的默认:

| 包 | 作用 |
| --- | --- |
| `xunit.v3` | 完整框架,带上当前默认的 MTP 集成(4.0 起默认是 MTP v2) |
| `xunit.v3.mtp-v2` | 显式选择 MTP v2 集成 —— **本教程用这个** |
| `xunit.v3.mtp-off` | 关闭 MTP 集成(想在 .NET 10 SDK 上继续用 VSTest 时才需要) |
| `xunit.v3.core` / `xunit.v3.core.*` | 核心引擎包(不含断言库与分析器);上面那些完整入口会把它一起拉进来 |

`xunit.v3.mtp-v1` 只存在于 3.x 系列,4.0 起已经不再支持 MTP v1。

写测试只需要引用上面其中一个包。`xunit.v3.mtp-v2` 的包描述里写明了它会安装 `xunit.v3.core.mtp-v2`、`xunit.v3.assert` 和 `xunit.analyzers`(2.2.0)—— 分析器随框架包一起进入编译过程,不需要单独安装,1.6 节会看到它确实在工作。

## 1.2 安装 v3 模板并创建工程

先确认 SDK:

```shell
$ dotnet --version
10.0.113
```

xUnit.net v3 的项目模板是一个独立的模板包,首次使用前要安装:

```shell
dotnet new install xunit.v3.templates
```

装好之后可以用 `dotnet new list xunit` 确认:

```shell
$ dotnet new list xunit
These templates matched your input: 'xunit'

Template Name                   Short Name        Language    Tags
------------------------------  ----------------  ----------  ----------------------
xUnit Test Project              xunit             [C#],F#,VB  Test/xUnit/Desktop/Web
xUnit.net v3 Extension Project  xunit3-extension  [C#],F#,VB  Test/xUnit
xUnit.net v3 Test Project       xunit3            [C#],F#,VB  Test/xUnit/Desktop/Web
```

三个名字里,`xunit` 是 v2 的模板,`xunit3` 是 v3 测试工程模板,`xunit3-extension` 用来写 xUnit.net 自己的扩展(第 12 章)。模板有更新时用 `dotnet new update` 升级。

创建工程:

```shell
$ dotnet new xunit3 -f net10.0 -o Ch01.GettingStarted
The template "xUnit.net v3 Test Project" was created successfully.

Processing post-creation actions...
Restoring .../Ch01.GettingStarted/Ch01.GettingStarted.csproj:
  Determining projects to restore...
  Restored .../Ch01.GettingStarted/Ch01.GettingStarted.csproj (in 133 ms).
Restore succeeded.

Successfully modified global.json.
```

`-f, --framework` 的默认值是 `net8.0`(候选里还有 `net472`/`net48`/`net481`/`net9.0`/`net10.0`),所以必须显式写 `-f net10.0`。另外两个选项解释了本章后面会看到的现象:`dotnet test` 默认走 MTP,而 `dotnet run` 默认走 xUnit.net 自己的命令行。下面是 `dotnet new xunit3 -?` 输出里这两项的节选(每个选项的 `Enabled if:` 条件行已省略):

```text
  --test-runner <choice>      Test runner for 'dotnet test' and Test Explorer
                              Type: choice
                                mtp-v2  Microsoft.Testing.Platform v2
                                vstest  VSTest
                              Default: mtp-v2
  --command-line <mtp|xunit>  Command line experience for 'dotnet run'
                              Type: choice
                                xunit  xUnit.net native command line
                                mtp    Microsoft.Testing.Platform command line
                              Default: xunit
```

真实应用通常把生产代码与测试代码放在两个项目里,并用 solution 管理它们。常见布局是 `src/` + `tests/`(扁平布局也可以):

```shell
dotnet new sln -n MyProject
dotnet new classlib -n MyProject.Core -o src/MyProject.Core
dotnet new xunit3 -f net10.0 -n MyProject.Core.Tests -o tests/MyProject.Core.Tests
dotnet sln MyProject.slnx add src/MyProject.Core/MyProject.Core.csproj
dotnet sln MyProject.slnx add tests/MyProject.Core.Tests/MyProject.Core.Tests.csproj
dotnet add tests/MyProject.Core.Tests/MyProject.Core.Tests.csproj reference src/MyProject.Core/MyProject.Core.csproj
```

当前 SDK 的 `dotnet new sln` 默认创建 `.slnx`;若工具链要求传统 `.sln`,创建时加 `-f sln`,并把后续命令中的文件名相应改成 `MyProject.sln`。把文件放进 `src/` 和 `tests/` 目录**不会自动建立项目引用**;测试项目仍需 `ProjectReference`,而两个项目都要加入 solution。为集中讲解 runner 与测试方法,本章示例把 `Production/` 和测试文件放在同一个工程里;多项目目录与引用会在实际应用中更常见。

## 1.3 逐行解剖生成的工程

模板会生成 [`Ch01.GettingStarted.csproj`](../code/Ch01.GettingStarted/Ch01.GettingStarted.csproj)、`UnitTest1.cs`、`xunit.runner.json`,还会创建或更新一份 [`global.json`](../code/global.json)(就近选择:上层目录已有就改它,否则在项目目录里新建)。

### 项目文件

本章工程最终的项目文件:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
  </PropertyGroup>

  <ItemGroup>
    <Content Include="xunit.runner.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="xunit.v3.mtp-v2" Version="4.0.2" />
  </ItemGroup>

</Project>
```

逐项说明:

- `ImplicitUsings` + `<Using Include="Xunit" />`:除了 C# 默认的隐式 using,再隐式引入 `Xunit` 命名空间。这就是测试文件里可以不写 `using Xunit;` 而 `[Fact]`、`Assert` 依然可用的原因。
- `Nullable`:启用可空引用类型。xUnit.net 自身的库带 nullable 标注,例如 `Assert.NotNull` 被标注过,断言通过后编译器就知道这个值不为 `null`。
- `OutputType` 为 `Exe`:**v3 的测试工程是可执行程序**,构建产物能直接运行(本节末尾有与 v2 的对比)。这是 v3 与 v2 最大的结构性差别。
- `TargetFramework`:本章用 `net10.0`;v3 最低支持 `net8.0` 与 `net472`。
- `TestingPlatformDotnetTestSupport`:声明这个项目支持 MTP 版的 `dotnet test`。这个属性服务于 .NET 8/9 SDK;在 .NET 10 SDK 上真正起作用的是下面 [`global.json`](../code/global.json) 里的配置。如果本地模板生成的项目里没有这一行,补上即可。
- `xunit.runner.json` 作为 `Content` 复制到输出目录:runner 启动时会读取它。
- 唯一的包引用 `xunit.v3.mtp-v2`。模板当时生成的是 `4.0.1`,本章统一改成 `4.0.2`。

### global.json:让 `dotnet test` 走 MTP

[`code/global.json`](../code/global.json)(整个 `code` 树共用一份):

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

它的作用是:在 .NET 10 SDK 上,`dotnet test` 默认仍然会走老的 VSTest 通道,而 MTP 已经不再支持这个通道。把这份 [`global.json`](../code/global.json) 放在项目目录或其上层目录,`dotnet test` 才会切到 MTP 模式。缺了它,构建会直接失败:

```text
error : Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and later.
If you use dotnet test, you should opt-in to the new dotnet test experience.
For more information, see https://aka.ms/dotnet-test-mtp-error
```

这条错误出现在一个删掉 [`global.json`](../code/global.json) 的空白 v3 工程上,是本章实测到的输出。

### xunit.runner.json

```json
{
    "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json"
}
```

空壳而已,`$schema` 让编辑器能做补全和校验。这个文件两种 runner 都读,里面可以配 `culture`、`methodDisplay`、`diagnosticMessages`、`assertEquivalentMaxDepth` 等;MTP 模式下还额外支持 `testconfig.json`。配置项的细节在第 10 章。

### 独立可执行程序:与 v2 的对比

v2 的测试工程是**库**:编译出一个 DLL,自己跑不起来,必须由外部 runner(`xunit.console`、Visual Studio 的 Test Explorer、`dotnet test`)把它加载进自己的进程再执行。这套设计来自 .NET Framework 时代,靠 AppDomain 做 runner 与被测代码之间的隔离;而 .NET Core/.NET 没有 AppDomain 卸载能力,于是隔离与依赖解析都变成了框架要额外解决的麻烦。v3 直接把测试工程做成**可执行程序**:依赖解析交给编译器,隔离交给"另起一个进程"这个最朴素的手段。对你的直接影响是:构建产物自己就能跑测试,不需要先装某个 runner —— 这也是 `dotnet run` 能直接跑测试的原因。

对 .NET 项目还要注意:真正的测试代码在 `.dll` 里,同名的无扩展名可执行文件(Windows 上是 `.exe`)只是一个启动用的壳。凡是需要传"测试程序集"路径的多工程 runner,要传 `.dll`,不是那个壳。

## 1.4 第一个测试:一个通过,一个故意失败

模板生成的 `UnitTest1.cs` 只有一个 `Assert.True(true)`,把它删掉。本章的被测代码放在 [`Production/`](../code/Ch01.GettingStarted/Production/) 目录,测试代码放在工程根目录,命名空间分别是 `Ch01.GettingStarted.Production` 与 `Ch01.GettingStarted.Tests`。

### 被测代码:[`Production/PriceCalculator.cs`](../code/Ch01.GettingStarted/Production/PriceCalculator.cs)

```csharp
namespace Ch01.GettingStarted.Production;

/// <summary>
/// 本章的被测对象:把购物车里各单品的价格汇总,套用会员折扣,再算出含税总价。
/// 逻辑刻意保持简单,读者的注意力可以全部放在测试本身。
/// </summary>
public class PriceCalculator
{
    private const decimal TaxRate = 0.13m;
    private const decimal DiscountThreshold = 100m;
    private const decimal DiscountAmount = 20m;

    /// <summary>各单品价格合计。</summary>
    public decimal Subtotal(IEnumerable<decimal> itemPrices)
    {
        ArgumentNullException.ThrowIfNull(itemPrices);
        return itemPrices.Sum();
    }

    /// <summary>会员折扣:合计满 100 减 20,不满则原价。</summary>
    public decimal ApplyMemberDiscount(decimal subtotal)
    {
        if (subtotal < 0m)
            throw new ArgumentOutOfRangeException(nameof(subtotal), subtotal, "合计金额不能为负数。");

        return subtotal >= DiscountThreshold ? subtotal - DiscountAmount : subtotal;
    }

    /// <summary>按 13% 的税率计算含税总价,四舍五入到分。</summary>
    public decimal TotalWithTax(decimal netAmount)
    {
        if (netAmount < 0m)
            throw new ArgumentOutOfRangeException(nameof(netAmount), netAmount, "金额不能为负数。");

        return Math.Round(netAmount * (1m + TaxRate), 2, MidpointRounding.AwayFromZero);
    }
}
```

### 测试代码:[`PriceCalculatorTests.cs`](../code/Ch01.GettingStarted/PriceCalculatorTests.cs)

```csharp
using Ch01.GettingStarted.Production;

namespace Ch01.GettingStarted.Tests;

public class PriceCalculatorTests
{
    private readonly PriceCalculator calculator = new();

    [Fact]
    public void Subtotal_AddsAllItemPrices()
    {
        var itemPrices = new[] { 19.90m, 5.00m, 75.05m };

        var subtotal = calculator.Subtotal(itemPrices);

        Assert.Equal(99.95m, subtotal);
    }

    [Fact]
    public void ApplyMemberDiscount_Reduces20WhenSubtotalReaches100()
    {
        var discounted = calculator.ApplyMemberDiscount(128.00m);

        Assert.Equal(108.00m, discounted);
    }

    // 故意写错的期望值:80 * 1.13 = 90.40,这里写成 90.30,用来演示失败输出长什么样。
    // 去掉 Skip 参数即可复现;带 Skip 是为了让本章工程始终全绿。
    [Fact(Skip = "演示用:故意失败的断言,去掉 Skip 参数即可复现")]
    public void TotalWithTax_AddsThirteenPercentTax()
    {
        var total = calculator.TotalWithTax(80.00m);

        Assert.Equal(90.30m, total);
    }
}
```

三个要点:

1. `[Fact]` 标记一个"无参数、恒为真"的测试。这是 xUnit.net 的写法,和 MSTest/NUnit 的 `[Test]` 是同一件事。
2. 测试类里的字段(`calculator`)在每个测试方法执行前都是全新的:xUnit.net **为每个测试新建一个测试类实例**,所以测试之间不会互相污染状态。这也是可以放心用字段做公共准备的原因(第 2、5 章展开)。
3. 断言写成"期望值在前,实际值在后":`Assert.Equal(expected, actual)`。失败输出的 `Expected`/`Actual` 就是按这个顺序打印的,写反了会把失败信息读反。

第三个测试带 `Skip`,是为了让本章工程始终全绿;把它去掉就能看到下面这个失败。

### 失败输出怎么读

用 `dotnet run`(xUnit.net 原生 runner)跑一次:

```text
xUnit.net v3 In-Process Runner v4.0.2+0c7aa4991f (64-bit .NET 10.0.13)
  Discovering: Ch01.GettingStarted
  Discovered:  Ch01.GettingStarted
  Starting:    Ch01.GettingStarted
    Ch01.GettingStarted.Tests.PriceCalculatorTests.TotalWithTax_AddsThirteenPercentTax [FAIL]
      Assert.Equal() Failure: Values differ
      Expected: 90.30
      Actual:   90.40
      Stack Trace:
        Ch01.GettingStarted/PriceCalculatorTests.cs(34,0): at Ch01.GettingStarted.Tests.PriceCalculatorTests.TotalWithTax_AddsThirteenPercentTax()
           at System.Reflection.MethodBaseInvoker.InterpretedInvoke_Method(Object obj, IntPtr* args)
           at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)
  Finished:    Ch01.GettingStarted (ID = '9fbe81a65dd55c0f07e4b4e2b335d122f7ee04b56905ee32658c1d08220488cd')
=== TEST EXECUTION SUMMARY ===
   Ch01.GettingStarted  Total: 3, Errors: 0, Failed: 1, Skipped: 0, Not Run: 0, Time: 0.089s
```

从上往下读:

- 测试的完整名字是 `命名空间.类名.方法名`,后面直接跟 `[FAIL]`;数据驱动测试还会带上 `(value: 6)` 这样的参数值,好让你知道是哪一行数据失败。
- `Expected: 90.30` / `Actual: 90.40`:断言里第一个参数是 Expected,第二个是 Actual。这里期望值写错,代码是对的 —— 修测试而不是修代码。
- `Ch01.GettingStarted/PriceCalculatorTests.cs(34,0)`:文件与行号(行,列),正好指向 `Assert.Equal(90.30m, total);` 那一行。
- 最后两行 `System.Reflection.MethodBaseInvoker...` 是 xUnit.net 反射调用测试方法留下的框架栈帧,可以忽略。
- 汇总行 `Total: 3, Errors: 0, Failed: 1, Skipped: 0, Not Run: 0`:总数、错误数(清理阶段失败,比如夹具或测试类的 `Dispose` 抛异常,以及 runner 报告的系统级错误)、失败数(断言失败或未捕获异常 —— 注意测试体内和测试类构造函数里抛出的异常都计入 Failed,而不是 Errors)、被跳过的测试、以及没被运行的测试(比如显式标记为 `Explicit` 的)。进程退出码非 0,所以 CI 会红。

同一个失败在 `dotnet test`(MTP)下的输出形状不同,信息是一样的:

```text
failed Ch01.GettingStarted.Tests.PriceCalculatorTests.TotalWithTax_AddsThirteenPercentTax (1ms)
  from .../Ch01.GettingStarted/bin/Debug/net10.0/Ch01.GettingStarted.dll (net10.0|x64)
  Assert.Equal() Failure: Values differ
  Expected: 90.30
  Actual:   90.40
    at Ch01.GettingStarted.Tests.PriceCalculatorTests.TotalWithTax_AddsThirteenPercentTax() in .../Ch01.GettingStarted/PriceCalculatorTests.cs:34
```

MTP 用 `文件:行号` 的形式(注意是 [`PriceCalculatorTests.cs:34`](../code/Ch01.GettingStarted/PriceCalculatorTests.cs#L34)),并在末尾给出 `Test run summary` 与退出码。

## 1.5 三种运行方式

以下命令都在 `code/` 目录下执行。

### 方式一:`dotnet test`(MTP 模式)

```shell
$ dotnet test --project Ch01.GettingStarted
Running tests from .../Ch01.GettingStarted/bin/Debug/net10.0/Ch01.GettingStarted.dll (net10.0|x64)
skipped Ch01.GettingStarted.Tests.AnalyzerDemoTests.AsyncVoidTest (0ms)
  演示用:async void 在 v3 会被运行时快速失败,修复见下一个测试
  from .../Ch01.GettingStarted/bin/Debug/net10.0/Ch01.GettingStarted.dll (net10.0|x64)
  ...
.../Ch01.GettingStarted/bin/Debug/net10.0/Ch01.GettingStarted.dll (net10.0|x64) passed (2s 268ms)

Test run summary: Passed!
  total: 5
  failed: 0
  succeeded: 3
  skipped: 2
  duration: 2s 390ms
```

这是 IDE、CI 脚本最常用的入口:它会先构建再运行,汇总用 `total / failed / succeeded / skipped`,退出码 0 表示成功。

一个容易踩的坑:.NET 10 SDK 不接受把工程路径作为裸参数传给 `dotnet test`:

```text
$ dotnet test Ch01.GettingStarted
Specifying a directory for 'dotnet test' should be via '--project' or '--solution'.
```

要写 `--project`(或 `--solution`)。在工程目录里直接 `dotnet test` 也可以。

### 方式二:`dotnet run`(xUnit.net 原生命令行)

```shell
$ dotnet run --project Ch01.GettingStarted
xUnit.net v3 In-Process Runner v4.0.2+0c7aa4991f (64-bit .NET 10.0.13)
  Discovering: Ch01.GettingStarted
  Discovered:  Ch01.GettingStarted
  Starting:    Ch01.GettingStarted
    Ch01.GettingStarted.Tests.PriceCalculatorTests.TotalWithTax_AddsThirteenPercentTax [SKIP]
      演示用:故意失败的断言,去掉 Skip 参数即可复现
  ...
  Finished:    Ch01.GettingStarted (ID = '9fbe81a65dd55c0f07e4b4e2b335d122f7ee04b56905ee32658c1d08220488cd')
=== TEST EXECUTION SUMMARY ===
   Ch01.GettingStarted  Total: 5, Errors: 0, Failed: 0, Skipped: 2, Not Run: 0, Time: 0.085s
```

汇总行的读法:

- `Total: 5` 是发现到的测试总数:本章两个文件里一共 5 个 `[Fact]` 方法,就是 5 个测试(数据驱动测试会把一行数据算成一个测试,第 4 章)。`Errors`/`Failed`/`Skipped`/`Not Run` 含义同上。
- `[SKIP]` 下面缩进的那一行就是 `Skip = "..."` 里的文字 —— 跳过原因必须写清楚,否则半年后没人知道为什么跳过。
- `Finished` 行括号里的 `ID` 是测试程序集的标识:同一工程原地反复运行它不变。实测它与测试代码无关(删掉一个测试文件再跑,它不变),但把整个工程复制到别的位置后它就变了。
- 全部通过时退出码是 0;有失败时是 1(1.4 那次就是 1)。

`Discovering` / `Discovered` 两行说明执行分两个阶段:先发现(扫描 `[Fact]`/`[Theory]` 并展开数据行),再执行。测试数量与预期不符时,先看发现阶段。

### 方式三:直接运行构建产物

```shell
$ dotnet build Ch01.GettingStarted
...
Build succeeded.
    0 Warning(s)
    0 Error(s)

$ cd Ch01.GettingStarted/bin/Debug/net10.0
$ ./Ch01.GettingStarted
```

输出与方式二完全一样(同一个 runner,只是不经过 `dotnet run` 的构建步骤)。这就是"测试工程是独立可执行程序"的实际收益:不需要先装任何 runner,构建产物自己就能跑测试。

### 传命令行选项:两种命令行,别混

`dotnet run` 和 `dotnet build` 会把 `--` 之后的参数交给你的程序,所以选项要写在 `--` 后面:

```shell
$ dotnet run --project Ch01.GettingStarted -- -?
```

`-?` 输出的是 xUnit.net 原生 runner 的选项,用法行和筛选类选项长这样(中间省略):

```text
usage: [:seed] [path/to/configFile.json] [options] [filters] [reporter] [resultFormat filename [...]]

General options
  ...

  -class "name"        : run all methods in a given test class (type names are fully qualified;
                       : i.e., 'MyNamespace.MyClass' or 'MyNamespace.MyClass+InnerClass'; wildcard '*'
                       : is supported at the beginning and/or end of the filter)
                       :   if specified more than once, acts as an OR operation
```

用它只跑一个类:

```shell
$ ./Ch01.GettingStarted -class Ch01.GettingStarted.Tests.PriceCalculatorTests
xUnit.net v3 In-Process Runner v4.0.2+0c7aa4991f (64-bit .NET 10.0.13)
  Discovering: Ch01.GettingStarted
  ...
=== TEST EXECUTION SUMMARY ===
   Ch01.GettingStarted  Total: 3, Errors: 0, Failed: 0, Skipped: 1, Not Run: 0, Time: 0.081s
```

`Total` 从 5 变成 3,说明过滤器确实生效了。

`dotnet test` 走的是 MTP,选项风格是双横线,同样要放在 `--` 之后:

```shell
$ dotnet test --project Ch01.GettingStarted -- --filter-class "Ch01.GettingStarted.Tests.AnalyzerDemoTests"
Running tests from .../Ch01.GettingStarted/bin/Debug/net10.0/Ch01.GettingStarted.dll (net10.0|x64)
skipped Ch01.GettingStarted.Tests.AnalyzerDemoTests.AsyncVoidTest (0ms)
  演示用:async void 在 v3 会被运行时快速失败,修复见下一个测试
  from .../Ch01.GettingStarted/bin/Debug/net10.0/Ch01.GettingStarted.dll (net10.0|x64)
.../Ch01.GettingStarted/bin/Debug/net10.0/Ch01.GettingStarted.dll (net10.0|x64) passed (2s 326ms)

Test run summary: Passed!
  total: 2
  failed: 0
  succeeded: 1
  skipped: 1
  duration: 2s 447ms
```

两套选项**不能混用**。把 MTP 选项交给原生命令行会直接报错:

```shell
$ dotnet run --project Ch01.GettingStarted -- --filter-class "X"
error: unknown option: --filter-class
```

想记住哪套对应哪条命令,就记这一点:`dotnet test` 是 MTP(`--filter-class`、`--filter-method`、`--filter-trait`、`--report-xunit-junit`……),`dotnet run` 与直接跑产物是 xUnit.net 原生命令行(`-class`、`-method`、`-trait`、`-xml`……)。模板的 `--command-line mtp` 可以把 `dotnet run` 也换成 MTP 的命令行体验,那时才轮到双横线选项。

顺带一个实测到的 SDK 毛病:`dotnet test --project ... -- --help` 会抛 `InvalidOperationException: An unexpected help-related message was received when --help wasn't used.`(SDK 10.0.113)。要查 MTP 的选项,看 Microsoft Testing Platform 的文档,或者先用原生命令行的 `-?` 熟悉常用筛选与报告选项。

## 1.6 分析器:把常见错误拦在编译期

xUnit.net 的分析器随框架包一起参与编译,能在编译期发现"能编译但跑不对"的写法。本章工程里保留了一个反例,[`AnalyzerDemoTests.cs`](../code/Ch01.GettingStarted/AnalyzerDemoTests.cs):

```csharp
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
```

把中间那两行 `#pragma` 删掉再编译,会得到:

```text
.../Ch01.GettingStarted/AnalyzerDemoTests.cs(13,23): error xUnit1049: Support for 'async void' unit tests has been removed from xUnit.net v3. Convert the test to 'async Task' or 'async ValueTask' instead. (https://xunit.net/xunit.analyzers/rules/xUnit1049)
    1 Error(s)
```

(行号是删掉 `#pragma` 之后那份文件的行号。)

这条诊断值得单独说清楚,因为 `async void` 是 C# 里最典型的测试写法陷阱:

- `async void` 方法是"发射后不管"的:`await` 之后的代码在测试方法已经返回之后才执行,断言失败也冒不到 runner 手里,测试会假绿。
- v3 直接取消了对它的支持,运行时会把这类测试快速失败(fast-fail),所以本章给它加了 `Skip`。
- 分析器更进一步:它是**错误级**,连编译都过不去,位置精确到方法名所在列。错误消息还直接给出修复方向(`async Task` 或 `async ValueTask`)和规则链接。

这就是分析器的价值:不是风格洁癖,而是把"测试看起来通过了但其实什么都没测"这类静默错误变成编译失败。分析器规则很多(编号是 `xUnitNNNN`),常见的还有 `xUnit2002`(对值类型用 `Assert.Null`)、`xUnit1026`(数据驱动测试的参数没被用到)、`xUnit1051`(能接收 `CancellationToken` 的调用没传 token)。每条都带链接,遇到就点开看。

`#pragma warning disable` 只应该出现在演示代码里,并且要像上面那样写清原因、成对使用 `restore`。

## 动手练习

1. **修好那个故意失败的测试。** 去掉 `TotalWithTax_AddsThirteenPercentTax` 的 `Skip` 参数,把断言改对。
   验收:`dotnet test --project Ch01.GettingStarted` 汇总为 `total: 5`、`failed: 0`、`succeeded: 4`、`skipped: 1`(剩下那个 skipped 是 `AsyncVoidTest`,它是 1.6 节的演示,不要去掉它的 `Skip`)。

2. **用两种命令行各过滤一次。** 用原生选项只运行 `PriceCalculatorTests` 类,再用 MTP 选项只运行 `Subtotal_AddsAllItemPrices` 方法。
   验收:前者汇总显示 `Total: 3`;后者汇总显示 `total: 1`、`succeeded: 1`。命令模板见 1.5 节。

3. **自己触发一次分析器。** 新建一个测试方法,故意写成 `async void`,观察 `xUnit1049` 编译错误;然后按错误消息修复。
   验收:先看到 `error xUnit1049 ... 1 Error(s)`,修复后 `dotnet build Ch01.GettingStarted` 显示 `0 Warning(s)`、`0 Error(s)`。

4. **为免邮规则补一组测试。** 在独立生产项目中实现 `FreeShippingPolicy.IsEligible(decimal subtotal)`。先分别测试未达到门槛、刚好达到门槛和无效金额，再实现规则;测试与生产代码放在不同项目,用 `ProjectReference` 连接。此练习只需 `[Fact]`、`Assert.True`、`Assert.False` 与 `Assert.Throws`。

## 本章小结

- xUnit.net v3(4.0.2)是主线,v2(2.9.3)只是维护;新工程引用 `xunit.v3.mtp-v2` 一个包就够,分析器随之而来。
- 用 `dotnet new install xunit.v3.templates` 装模板,`dotnet new xunit3 -f net10.0` 建工程;模板默认框架是 `net8.0`,要显式指定。
- 项目文件里的 `OutputType Exe` 是关键:v3 测试工程是独立可执行程序,自己就能跑测试,不再需要外部 runner 把库加载进自己的进程。
- [`global.json`](../code/global.json) 里的 `"test": { "runner": "Microsoft.Testing.Platform" }` 是 .NET 10 SDK 上让 `dotnet test` 能用的前提;缺了它会报 "Testing with VSTest target is no longer supported..."。
- `dotnet test` 走 MTP(汇总 `total/failed/succeeded/skipped`,失败退出码 2),`dotnet run` 与直接跑产物走 xUnit.net 原生命令行(汇总 `=== TEST EXECUTION SUMMARY ===`,失败退出码 1);两者选项风格不同,都必须放在 `--` 之后。
- 失败输出里 `Expected` 是断言的第一个参数,`Actual` 是第二个;紧跟其后的是 `文件(行,列)` 或 `文件:行号`,直接指向失败的那一行断言。
- 每个测试都在测试类的新实例里运行,所以测试类字段天然是"每个测试一份干净副本"。
- 分析器把 `async void` 这类静默错误拦在编译期(v3 已不支持 `async void`);跳过测试一定要写原因。

## 本章代码

```text
code/Ch01.GettingStarted/
├── Ch01.GettingStarted.csproj
├── xunit.runner.json
├── Production/PriceCalculator.cs
├── PriceCalculatorTests.cs
└── AnalyzerDemoTests.cs
```

运行:

```bash
cd code

dotnet test --project Ch01.GettingStarted          # MTP 模式
dotnet run --project Ch01.GettingStarted           # xUnit.net 原生命令行
dotnet run --project Ch01.GettingStarted -- -?     # 原生 runner 的全部选项
```

本章定义了后续章节沿用的约定:被测代码放在工程内 [`Production/`](../code/Ch01.GettingStarted/Production/) 目录、命名空间 `ChNN.*.Production`;测试命名空间 `ChNN.*.Tests`;需要故意失败或跳过的演示用例用 `[Fact(Skip = "原因")]` 标注,保证章工程始终全绿。
