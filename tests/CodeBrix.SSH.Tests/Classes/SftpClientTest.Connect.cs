using System.Net.Sockets;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SftpClientTest_Connect
{
    [Fact]
    public void Connect_HostNameInvalid_ShouldThrowSocketExceptionWithErrorCodeHostNotFound()
    {
        var connectionInfo = new ConnectionInfo("invalid.", 40, "user",
            new KeyboardInteractiveAuthenticationMethod("user"));
        var sftpClient = new SftpClient(connectionInfo);

        try
        {
            sftpClient.Connect();
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SocketException ex)
        {
            Assert.True(ex.SocketErrorCode is SocketError.HostNotFound or SocketError.TryAgain or SocketError.NoData);
        }
    }

    [Fact]
    public void Connect_ProxyHostNameInvalid_ShouldThrowSocketExceptionWithErrorCodeHostNotFound()
    {
        var connectionInfo = new ConnectionInfo("localhost", 40, "user", ProxyTypes.Http, "invalid.", 80,
            "proxyUser", "proxyPwd", new KeyboardInteractiveAuthenticationMethod("user"));
        var sftpClient = new SftpClient(connectionInfo);

        try
        {
            sftpClient.Connect();
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SocketException ex)
        {
            Assert.True(ex.SocketErrorCode is SocketError.HostNotFound or SocketError.TryAgain or SocketError.NoData);
        }
    }
}
