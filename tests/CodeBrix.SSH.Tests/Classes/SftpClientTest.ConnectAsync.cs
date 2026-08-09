using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public partial class SftpClientTest
{
    [Fact]
    public async Task ConnectAsync_HostNameInvalid_ShouldThrowSocketExceptionWithErrorCodeHostNotFound()
    {
        var connectionInfo = new ConnectionInfo(Guid.NewGuid().ToString("N"), 40, "user",
            new KeyboardInteractiveAuthenticationMethod("user"));
        var sftpClient = new SftpClient(connectionInfo);

        try
        {
            await sftpClient.ConnectAsync(TestContext.Current.CancellationToken);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SocketException ex)
        {
            Assert.True(ex.SocketErrorCode is SocketError.HostNotFound or SocketError.TryAgain, $"Socket error is {ex.SocketErrorCode}");
        }
    }

    [Fact]
    public async Task ConnectAsync_ProxyHostNameInvalid_ShouldThrowSocketExceptionWithErrorCodeHostNotFound()
    {
        var connectionInfo = new ConnectionInfo("localhost", 40, "user", ProxyTypes.Http, Guid.NewGuid().ToString("N"), 80,
            "proxyUser", "proxyPwd", new KeyboardInteractiveAuthenticationMethod("user"));
        var sftpClient = new SftpClient(connectionInfo);

        try
        {
            await sftpClient.ConnectAsync(TestContext.Current.CancellationToken);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SocketException ex)
        {
            Assert.True(ex.SocketErrorCode is SocketError.HostNotFound or SocketError.TryAgain, $"Socket error is {ex.SocketErrorCode}");
        }
    }
}
