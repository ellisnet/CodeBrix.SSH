using System;
using System.Net;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.SSH.Tests.Properties;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Provides connection information when password authentication method is used
/// </summary>
public class PasswordConnectionInfoTest : TestBase
{
    [Fact]
    public void Test_ConnectionInfo_Host_Is_Null()
    {
        try
        {
            _ = new PasswordConnectionInfo(null, Resources.USERNAME, Resources.PASSWORD);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("host", ex.ParamName);
        }

    }

    [Fact]
    public void Test_ConnectionInfo_Username_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new PasswordConnectionInfo(Resources.HOST, null, Resources.PASSWORD));
    }

    [Fact]
    public void Test_ConnectionInfo_Password_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(
            () => new PasswordConnectionInfo(Resources.HOST, Resources.USERNAME, (string)null));
    }

    [Fact]
    public void Test_ConnectionInfo_Username_Is_Whitespace()
    {
        Assert.Throws<ArgumentException>(() => new PasswordConnectionInfo(Resources.HOST, " ", Resources.PASSWORD));
    }

    [Fact]
    public void Test_ConnectionInfo_SmallPortNumber()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MinPort - 1, Resources.USERNAME, Resources.PASSWORD));
    }

    [Fact]
    public void Test_ConnectionInfo_BigPortNumber()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MaxPort + 1, Resources.USERNAME, Resources.PASSWORD));
    }
}
