using System;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.SSH.Tests.Properties;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Provides functionality for local port forwarding
/// </summary>
public partial class ForwardedPortLocalTest : TestBase
{
    [Fact]
    public void ConstructorShouldThrowArgumentNullExceptionWhenBoundHostIsNull()
    {
        try
        {
            _ = new ForwardedPortLocal(null, 8080, Resources.HOST, 80);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("boundHost", ex.ParamName);
        }
    }

    [Fact]
    public void ConstructorShouldNotThrowExceptionWhenBoundHostIsEmpty()
    {
        var boundHost = string.Empty;

        var forwardedPort = new ForwardedPortLocal(boundHost, 8080, Resources.HOST, 80);

        Assert.Same(boundHost, forwardedPort.BoundHost);
    }

    [Fact]
    public void ConstructorShouldNotThrowExceptionWhenBoundHostIsInvalidDnsName()
    {
        const string boundHost = "in_valid_host.";

        var forwardedPort = new ForwardedPortLocal(boundHost, 8080, Resources.HOST, 80);

        Assert.Same(boundHost, forwardedPort.BoundHost);
    }

    [Fact]
    public void ConstructorShouldThrowArgumentNullExceptionWhenHostIsNull()
    {
        try
        {
            _ = new ForwardedPortLocal(Resources.HOST, 8080, null, 80);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("host", ex.ParamName);
        }
    }

    [Fact]
    public void ConstructorShouldNotThrowExceptionWhenHostIsEmpty()
    {
        var host = string.Empty;

        var forwardedPort = new ForwardedPortLocal(Resources.HOST, 8080, string.Empty, 80);

        Assert.Same(host, forwardedPort.Host);
    }

    [Fact]
    public void ConstructorShouldNotThrowExceptionWhenHostIsInvalidDnsName()
    {
        const string host = "in_valid_host.";

        var forwardedPort = new ForwardedPortLocal(Resources.HOST, 8080, host, 80);

        Assert.Same(host, forwardedPort.Host);
    }
}
