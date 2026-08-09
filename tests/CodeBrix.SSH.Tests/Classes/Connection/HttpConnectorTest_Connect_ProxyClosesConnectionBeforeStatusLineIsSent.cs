using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Connection; //was previously: Renci.SshNet.Tests.Classes.Connection;

public class HttpConnectorTest_Connect_ProxyClosesConnectionBeforeStatusLineIsSent : HttpConnectorTestBase
{
    private ConnectionInfo _connectionInfo;
    private AsyncSocketListener _proxyServer;
    private Socket _clientSocket;
    private bool _disconnected;
    private ProxyException _actualException;

    protected override void SetupData()
    {
        base.SetupData();

        _connectionInfo = new ConnectionInfo(IPAddress.Loopback.ToString(),
                                             777,
                                             "user",
                                             ProxyTypes.Http,
                                             IPAddress.Loopback.ToString(),
                                             8122,
                                             "proxyUser",
                                             "proxyPwd",
                                             new KeyboardInteractiveAuthenticationMethod("user"))
        {
            Timeout = TimeSpan.FromMilliseconds(100)
        };
        _actualException = null;

        _clientSocket = SocketFactory.Create(SocketType.Stream, ProtocolType.Tcp);

        _proxyServer = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, _connectionInfo.ProxyPort));
        _proxyServer.Disconnected += socket => _disconnected = true;
        _proxyServer.BytesReceived += (bytesReceived, socket) =>
            {
                socket.Shutdown(SocketShutdown.Send);
            };
        _proxyServer.Start();
    }

    protected override void SetupMocks()
    {
        _ = SocketFactoryMock.Setup(p => p.Create(SocketType.Stream, ProtocolType.Tcp))
                             .Returns(_clientSocket);
    }

    protected override void TearDown()
    {
        base.TearDown();

        _proxyServer?.Dispose();
        _clientSocket?.Dispose();
    }

    protected override void Act()
    {
        try
        {
            _ = Connector.Connect(_connectionInfo);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ProxyException ex)
        {
            _actualException = ex;
        }

        // Give some time to process all messages
        Thread.Sleep(200);
    }

    [Fact]
    public void ConnectShouldHaveThrownProxyException()
    {
        Assert.NotNull(_actualException);
        Assert.Null(_actualException.InnerException);
        Assert.Equal("HTTP response does not contain status line.", _actualException.Message);
    }

    [Fact]
    public void ConnectionToProxyShouldHaveBeenShutDown()
    {
        Assert.True(_disconnected);
    }

    [Fact]
    public void ClientSocketShouldHaveBeenDisposed()
    {
        try
        {
            _ = _clientSocket.Receive(new byte[0]);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ObjectDisposedException)
        {
        }
    }

    [Fact]
    public void CreateOnSocketFactoryShouldHaveBeenInvokedOnce()
    {
        SocketFactoryMock.Verify(p => p.Create(SocketType.Stream, ProtocolType.Tcp),
                                 Times.Once());
    }
}
