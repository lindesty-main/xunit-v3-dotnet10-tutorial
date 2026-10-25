using Ch02.TestStructure.Production;

namespace Ch02.TestStructure.Tests;

/// <summary>
/// 本章正文反例的存放处:这些测试本身是通过的,但写法故意不好。
/// 每个测试的注释标明它对应正文的哪一节。
/// </summary>
public class TestStructureAntiPatternTests
{
    // 2.2 节的反例:AAA 三步挤在一行里,名字没有信息。
    // 重构后的版本是 UsernamePolicyTests.Validate_UsernameBlank_ReportsRequiredViolation。
    [Fact]
    public void TestValidate_Empty()
    {
        var p = new UsernamePolicy(); var v = p.Validate(""); Assert.True(v.Count > 0);
    }

    // 2.3 节的反例:名字只记录了"测了哪个分支",没有记录"验证了什么行为"。
    [Fact]
    public void ValidateUsername_Case1_ReturnsTrue()
    {
        var violations = new UsernamePolicy().Validate("alice");

        Assert.Empty(violations);
    }

    // 2.4 节的反例:把两套校验逻辑和一个布尔标志挤进同一个 Theory。
    // 失败时输出里只剩 True/False,看不出输入到底违反了哪条规则。
    [Theory]
    [InlineData("alice", "Str0ngPass", true)]
    [InlineData("ab", "Str0ngPass", false)]
    [InlineData("alice", "weak", false)]
    public void Validate_UsernameAndPassword_ReturnsExpectedFlag(string username, string password, bool expected)
    {
        var usernameOk = new UsernamePolicy().Validate(username).Count == 0;
        var passwordOk = new PasswordPolicy().Validate(password, username).Count == 0;

        Assert.Equal(expected, usernameOk && passwordOk);
    }
}

// 2.5 节"测试类不是继承层次"的反例:[Fact] 写在基类里,每个派生测试类
// 都会重跑它一遍。本工程只有一个派生类;再加一个,这条测试就会跑两遍。
public abstract class PolicyTestsBase
{
    [Fact]
    public void BaseClassFact_RunsInEveryDerivedTestClass()
    {
        Assert.True(true);
    }
}

public class DerivedFromBaseA : PolicyTestsBase
{
}
