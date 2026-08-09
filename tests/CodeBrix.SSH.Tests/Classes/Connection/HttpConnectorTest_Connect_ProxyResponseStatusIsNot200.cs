using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Connection; //was previously: Renci.SshNet.Tests.Classes.Connection;

public class HttpConnectorTest_Connect_ProxyResponseStatusIsNot200 : HttpConnectorTestBase
{
    private ConnectionInfo _connectionInfo;
    private AsyncSocketListener _proxyServer;
    private Socket _clientSocket;
    private List<byte> _bytesReceivedByProxy;
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
        _bytesReceivedByProxy = new List<byte>();
        _actualException = null;

        _clientSocket = SocketFactory.Create(SocketType.Stream, ProtocolType.Tcp);

        _proxyServer = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, _connectionInfo.ProxyPort));
        _proxyServer.Disconnected += (socket) => _disconnected = true;
        _proxyServer.BytesReceived += (bytesReceived, socket) =>
            {
                if (_bytesReceivedByProxy.Count == 0)
                {
                    _ = socket.Send(Encoding.ASCII.GetBytes("HTTP/1.0 404 I searched everywhere, really...\r\n"));

                    socket.Shutdown(SocketShutdown.Send);
                }

                _bytesReceivedByProxy.AddRange(bytesReceived);
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
        Assert.Equal("HTTP: Status code 404, \"I searched everywhere, really...\"", _actualException.Message);
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
