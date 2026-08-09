using System;
using System.Globalization;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortDynamicTest : TestBase
{
    [Fact]
    public void Constructor_HostAndPort()
    {
        var host = new Random().Next().ToString(CultureInfo.InvariantCulture);
        var port = (uint)new Random().Next(0, int.MaxValue);

        var target = new ForwardedPortDynamic(host, port);

        Assert.Same(host, target.BoundHost);
        Assert.Equal(port, target.BoundPort);
    }

    [Fact]
    public void Constructor_Port()
    {
        var port = (uint)new Random().Next(0, int.MaxValue);

        var target = new ForwardedPortDynamic(port);

        Assert.Same(string.Empty, target.BoundHost);
        Assert.Equal(port, target.BoundPort);
    }
}
