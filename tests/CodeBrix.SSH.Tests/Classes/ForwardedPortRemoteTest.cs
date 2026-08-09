using System;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Provides functionality for remote port forwarding
/// </summary>
public partial class ForwardedPortRemoteTest : TestBase
{
    [Fact]
    public void Start_NotAddedToClient()
    {
        const int boundPort = 80;
        var host = string.Empty;
        const uint port = 22;
        var target = new ForwardedPortRemote(boundPort, host, port);

        try
        {
            target.Start();
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (InvalidOperationException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("Forwarded port is not added to a client.", ex.Message);
        }
    }
}
