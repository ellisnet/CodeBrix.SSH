using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Connection; //was previously: Renci.SshNet.Tests.Classes.Connection;

public class HttpConnectorTest_Connect_TimeoutReadingStatusLine : HttpConnectorTestBase
{
    private ConnectionInfo _connectionInfo;
    private SshOperationTimeoutException _actualException;
    private Socket _clientSocket;
    private AsyncSocketListener _proxyServer;
    private Stopwatch _stopWatch;
    private AsyncSocketListener _server;
    private bool _disconnected;

    protected override void SetupData()
    {
        base.SetupData();

        var random = new Random();

        _connectionInfo = new ConnectionInfo(IPAddress.Loopback.ToString(),
                                             1028,
                                             "user",
                                             ProxyTypes.Http,
                                             IPAddress.Loopback.ToString(),
                                             8122,
                                             "proxyUser",
                                             "proxyPwd",
                                             new KeyboardInteractiveAuthenticationMethod("user"))
        {
            Timeout = TimeSpan.FromMilliseconds(random.Next(50, 200))
        };
        _stopWatch = new Stopwatch();
        _actualException = null;

        _clientSocket = SocketFactory.Create(SocketType.Stream, ProtocolType.Tcp);

        _proxyServer = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, _connectionInfo.ProxyPort));
        _proxyServer.Disconnected += (socket) => _disconnected = true;
        _proxyServer.Start();

        _server = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, _connectionInfo.Port));
        _server.Start();
    }

    protected override void SetupMocks()
    {
        _ = SocketFactoryMock.Setup(p => p.Create(SocketType.Stream, ProtocolType.Tcp))
                             .Returns(_clientSocket);
    }

    protected override void TearDown()
    {
        base.TearDown();

        _server?.Dispose();
        _proxyServer?.Dispose();
    }

    protected override void Act()
    {
        _stopWatch.Start();

        try
        {
            _ = Connector.Connect(_connectionInfo);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshOperationTimeoutException ex)
        {
            _actualException = ex;
        }
        finally
        {
            _stopWatch.Stop();
        }

        // Give some time to process all messages
        Thread.Sleep(200);
    }

    [Fact]
    public void ConnectShouldHaveThrownSshOperationTimeoutException()
    {
        Assert.Null(_actualException.InnerException);
        Assert.Equal(string.Format(CultureInfo.InvariantCulture, "Socket read operation has timed out after {0:F0} milliseconds.", _connectionInfo.Timeout.TotalMilliseconds), _actualException.Message);
    }

    [Fact]
    public void ConnectShouldHaveRespectedTimeout()
    {
        var errorText = string.Format("Elapsed: {0}, Timeout: {1}",
                                      _stopWatch.ElapsedMilliseconds,
                                      _connectionInfo.Timeout.TotalMilliseconds);

        // Compare elapsed time with configured timeout, allowing for a margin of error
        Assert.True(_stopWatch.ElapsedMilliseconds >= _connectionInfo.Timeout.TotalMilliseconds - 10, errorText);
        Assert.True(_stopWatch.ElapsedMilliseconds < _connectionInfo.Timeout.TotalMilliseconds + 100, errorText);
    }

    [Fact]
    public void ClientSocketShouldNotBeConnected()
    {
        Assert.True(_disconnected);
        Assert.False(_clientSocket.Connected);
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
