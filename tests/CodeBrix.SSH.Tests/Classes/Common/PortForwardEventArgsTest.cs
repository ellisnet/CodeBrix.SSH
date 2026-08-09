using System;
using System.Net;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.SSH.Tests.Properties;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Common; //was previously: Renci.SshNet.Tests.Classes.Common;

/// <summary>
/// Provides data for <see cref="ForwardedPort.RequestReceived"/> event.
/// </summary>
public class PortForwardEventArgsTest : TestBase
{
    [Fact]
    public void ConstructorShouldThrowArgumentNullExceptionWhenHostIsNull()
    {
        try
        {
            _ = new PortForwardEventArgs(null, 80);
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

        var eventArgs = new PortForwardEventArgs(host, 80);

        Assert.Same(host, eventArgs.OriginatorHost);
    }

    [Fact]
    public void ConstructorShouldNotThrowExceptionWhenHostIsInvalidDnsName()
    {
        const string host = "in_valid_host.";

        var eventArgs = new PortForwardEventArgs(host, 80);

        Assert.Same(host, eventArgs.OriginatorHost);
    }

    [Fact]
    public void ConstructorShouldThrowArgumentOutOfRangeExceptionWhenPortIsGreaterThanMaximumValue()
    {
        const int port = IPEndPoint.MaxPort + 1;

        try
        {
            _ = new PortForwardEventArgs(Resources.HOST, port);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("port", ex.ParamName);
        }
    }
}
