using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ExpectActionTest : TestBase
{
    [Fact]
    public void Constructor_StringAndAction()
    {
        var expect = new Random().Next().ToString(CultureInfo.InvariantCulture);
        Action<string> action = Console.WriteLine;

        var target = new ExpectAction(expect, action);

        Assert.NotNull(target.Expect);
        Assert.Equal(expect, target.Expect.ToString());
        Assert.Same(action, target.Action);
    }

    [Fact]
    public void Constructor_RegexAndAction()
    {
        var expect = new Regex("^.*");
        Action<string> action = Console.WriteLine;

        var target = new ExpectAction(expect, action);

        Assert.Same(expect, target.Expect);
        Assert.Same(action, target.Action);
    }
}
