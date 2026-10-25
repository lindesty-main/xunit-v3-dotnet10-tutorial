using Xunit.Sdk;
using Xunit.v3;

// 4.0 的程序集级并行配置(特性类型在 Xunit.v3 命名空间,ParallelMode / ParallelAlgorithm
// 两个枚举在 Xunit.Sdk 命名空间)。它取代了 v2 与 v3 早期的
// [assembly: CollectionBehavior(DisableTestParallelization = …, MaxParallelThreads = …,
// ParallelAlgorithm = …)]——后者的并行属性在 4.0 已过时。
//
// 这里显式写出的正是框架默认值(不写任何特性时同样是 Collections),因此本章默认运行
// 行为不变;把 Mode 改成 None / All,或加上 MaxThreads / Algorithm,即可改变整个程序集的
// 默认并行行为:
//
//   [assembly: Parallelization(Mode = ParallelMode.All, MaxThreads = 4,
//       Algorithm = ParallelAlgorithm.Aggressive)]

[assembly: Parallelization(Mode = ParallelMode.Collections)]
