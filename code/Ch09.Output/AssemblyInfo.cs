using Xunit;

// 9.5:程序集级类排序器。它原样返回输入,不改变任何顺序;
// 注册它的唯一目的,是在"测试集合开始之前"这个早期阶段读写一次 KeyValueStorage。
[assembly: TestClassOrdererAttribute<Ch09.Output.Tests.RunMarkerOrderer>]
