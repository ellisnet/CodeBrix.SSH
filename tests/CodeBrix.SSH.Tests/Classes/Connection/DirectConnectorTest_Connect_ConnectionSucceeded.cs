using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Connection; //was previously: Renci.SshNet.Tests.Classes.Connection;

public class DirectConnectorTest_Connect_ConnectionSucceeded : DirectConnectorTestBase
{
    private ConnectionInfo _connectionInfo;
    private AsyncSocketListener _server;
    private Socket _clientSocket;
    private Stopwatch _stopWatch;
    private bool _disconnected;
    private Socket _actual;

    protected override void SetupData()
    {
        base.SetupData();

        var random = new Random();

        _connectionInfo = CreateConnectionInfo(IPAddress.Loopback.ToString());
        _connectionInfo.Timeout = TimeSpan.FromMilliseconds(random.Next(50, 200));
        _stopWatch = new Stopwatch();
        _disconnected = false;

        _clientSocket = SocketFactory.Create(SocketType.Stream, ProtocolType.Tcp);

        _server = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, _connectionInfo.Port));
        _server.Disconnected += (socket) => _disconnected = true;
        _server.Connected += (socket) => socket.Send(new byte[1] { 0x44 });
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
        _clientSocket?.Dispose();
    }

    protected override void Act()
    {
        _stopWatch.Start();

        try
        {
            _actual = Connector.Connect(_connectionInfo);
        }
        finally
        {
            _stopWatch.Stop();
        }
    }

    [Fact]
    public void ConnectShouldHaveReturnedSocketCreatedUsingSocketFactory()
    {
        Assert.NotNull(_actual);
        Assert.Same(_clientSocket, _actual);
    }

    [Fact]
    public void ConnectShouldHaveRespectedTimeout()
    {
        var errorText = string.Format("Elapsed: {0}, Timeout: {1}",
                                      _stopWatch.ElapsedMilliseconds,
                                      _connectionInfo.Timeout.TotalMilliseconds);

        // Compare elapsed time with configured timeout, allowing for a margin of error
        Assert.True(_stopWatch.ElapsedMilliseconds < _connectionInfo.Timeout.TotalMilliseconds + 100, errorText);
    }

    [Fact]
    public void ClientSocketShouldBeConnected()
    {
        Assert.True(_clientSocket.Connected);
        Assert.False(_disconnected);
    }

    [Fact]
    public void NoBytesShouldHaveBeenReadFromSocket()
    {
        var buffer = new byte[1];

        var bytesRead = _clientSocket.Receive(buffer);
        Assert.Equal(1, bytesRead);
        Assert.Equal(0x44, buffer[0]);
    }

    [Fact]
    public void CreateOnSocketFactoryShouldHaveBeenInvokedOnce()
    {
        SocketFactoryMock.Verify(p => p.Create(SocketType.Stream, ProtocolType.Tcp),
                                 Times.Once());
    }
}
