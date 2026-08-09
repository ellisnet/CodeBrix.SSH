using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Connection; //was previously: Renci.SshNet.Tests.Classes.Connection;

public class DirectConnectorTest_Connect_ConnectionRefusedByServer : DirectConnectorTestBase
{
    private ConnectionInfo _connectionInfo;
    private SocketException _actualException;
    private Socket _clientSocket;
    private Stopwatch _stopWatch;

    protected override void SetupData()
    {
        base.SetupData();

        _connectionInfo = CreateConnectionInfo(IPAddress.Loopback.ToString());
        _connectionInfo.Timeout = TimeSpan.FromMilliseconds(5000);
        _stopWatch = new Stopwatch();
        _actualException = null;

        _clientSocket = SocketFactory.Create(SocketType.Stream, ProtocolType.Tcp);
    }

    protected override void SetupMocks()
    {
        _ = SocketFactoryMock.Setup(p => p.Create(SocketType.Stream, ProtocolType.Tcp))
                             .Returns(_clientSocket);
    }

    protected override void TearDown()
    {
        base.TearDown();

        _clientSocket?.Dispose();
    }

    protected override void Act()
    {
        _stopWatch.Start();

        try
        {
            _ = Connector.Connect(_connectionInfo);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SocketException ex)
        {
            _actualException = ex;
        }
        finally
        {
            _stopWatch.Stop();
        }
    }

    [Fact]
    public void ConnectShouldHaveThrownSocketException()
    {
        Assert.Null(_actualException.InnerException);
        Assert.Equal(SocketError.ConnectionRefused, _actualException.SocketErrorCode);
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
