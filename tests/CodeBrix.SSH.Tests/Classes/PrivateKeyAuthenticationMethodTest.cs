using System;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Provides functionality to perform private key authentication.
/// </summary>
public class PrivateKeyAuthenticationMethodTest : TestBase
{
    [Fact]
    public void PrivateKey_Test_Pass_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new PrivateKeyAuthenticationMethod(null, null));
    }

    [Fact]
    public void PrivateKey_Test_Pass_PrivateKey_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new PrivateKeyAuthenticationMethod("username", null));
    }

    [Fact]
    public void PrivateKey_Test_Pass_Whitespace()
    {
        Assert.Throws<ArgumentException>(() => new PrivateKeyAuthenticationMethod(string.Empty, null));
    }
}
