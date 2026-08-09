using System;
using System.Globalization;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Provides functionality for "none" authentication method
/// </summary>
public class NoneAuthenticationMethodTest : TestBase
{
    [Fact]
    public void None_Test_Pass_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new NoneAuthenticationMethod(null));
    }

    [Fact]
    public void None_Test_Pass_Whitespace()
    {
        Assert.Throws<ArgumentException>(() => new NoneAuthenticationMethod(string.Empty));
    }

    [Fact]
    public void Name()
    {
        var username = new Random().Next().ToString(CultureInfo.InvariantCulture);
        var target = new NoneAuthenticationMethod(username);

        Assert.Equal("none", target.Name);
    }

    [Fact]
    public void Username()
    {
        var username = new Random().Next().ToString(CultureInfo.InvariantCulture);
        var target = new NoneAuthenticationMethod(username);

        Assert.Same(username, target.Username);
    }
}
