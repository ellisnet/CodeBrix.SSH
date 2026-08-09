using System;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Provides functionality to perform password authentication.
/// </summary>
public partial class PasswordAuthenticationMethodTest : TestBase
{
    [Fact]
    public void Password_Test_Pass_Null_Username()
    {
        Assert.Throws<ArgumentNullException>(() => new PasswordAuthenticationMethod(null, "valid"));
    }

    [Fact]
    public void Password_Test_Pass_Null_Password()
    {
        Assert.Throws<ArgumentNullException>(() => new PasswordAuthenticationMethod("valid", (string)null));
    }

    [Fact]
    public void Password_Test_Pass_Valid_Username_And_Password()
    {
        new PasswordAuthenticationMethod("valid", "valid");
    }

    [Fact]
    public void Password_Test_Pass_Whitespace()
    {
        Assert.Throws<ArgumentException>(() => new PasswordAuthenticationMethod(string.Empty, "valid"));
    }

    [Fact]
    public void Password_Test_Pass_Valid()
    {
        new PasswordAuthenticationMethod("valid", string.Empty);
    }
}
