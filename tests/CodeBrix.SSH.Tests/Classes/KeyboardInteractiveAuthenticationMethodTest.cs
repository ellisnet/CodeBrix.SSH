using System;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Provides functionality to perform keyboard interactive authentication.
/// </summary>
public partial class KeyboardInteractiveAuthenticationMethodTest : TestBase
{
    [Fact]
    public void Keyboard_Test_Pass_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new KeyboardInteractiveAuthenticationMethod(null));
    }

    [Fact]
    public void Keyboard_Test_Pass_Whitespace()
    {
        Assert.Throws<ArgumentException>(() => new KeyboardInteractiveAuthenticationMethod(string.Empty));
    }
}
