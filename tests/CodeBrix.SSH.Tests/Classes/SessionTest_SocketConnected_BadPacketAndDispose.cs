using System;
using System.Net;
using System.Net.Sockets;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Connection;
using CodeBrix.SSH.Messages.Transport;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_SocketConnected_BadPacketAndDispose : IDisposable
{
    private Mock<IServiceFactory> _serviceFactoryMock;
    private Mock<ISocketFactory> _socketFactoryMock;
    private Mock<IConnector> _connectorMock;
    private Mock<IProtocolVersionExchange> _protocolVersionExchangeMock;
    private ConnectionInfo _connectionInfo;
    private Session _session;
    private AsyncSocketListener _serverListener;
    private IPEndPoint _serverEndPoint;
    private Socket _serverSocket;
    private Socket _clientSocket;
    private SshConnectionException _actualException;
    private SocketFactory _socketFactory;

    public SessionTest_SocketConnected_BadPacketAndDispose()
    {
        Setup();
    }

    private void Setup()
    {
        Arrange();
        Act();
    }

    public void Dispose()
    {
        TearDown();
    }

    private void TearDown()
    {
        _serverListener?.Dispose();
    }

    protected void CreateMocks()
    {
        _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
        _socketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
        _connectorMock = new Mock<IConnector>(MockBehavior.Strict);
        _protocolVersionExchangeMock = new Mock<IProtocolVersionExchange>(MockBehavior.Strict);
    }

    protected void SetupData()
    {
        _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
        _connectionInfo = new ConnectionInfo(_serverEndPoint.Address.ToString(), _serverEndPoint.Port, "user", new PasswordAuthenticationMethod("user", "password"))
        {
            Timeout = TimeSpan.FromMilliseconds(200)
        };
        _actualException = null;
        _socketFactory = new SocketFactory();

        _serverListener = new AsyncSocketListener(_serverEndPoint);
        _serverListener.Connected += (socket) =>
            {
                _serverSocket = socket;

                // Since we're mocking the protocol version exchange, we can immediately send the bad
                // packet upon establishing the connection

                var badPacket = new byte[] { 0x0a, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05 };

                _ = _serverSocket.Send(badPacket, 0, badPacket.Length, SocketFlags.None);

                _serverSocket.Shutdown(SocketShutdown.Send);
            };
        _serverListener.Start();

        _session = new Session(_connectionInfo, _serviceFactoryMock.Object, _socketFactoryMock.Object);

        _clientSocket = new DirectConnector(_socketFactory, NullLoggerFactory.Instance).Connect(_connectionInfo);
    }

    protected void SetupMocks()
    {
        _ = _serviceFactoryMock.Setup(p => p.CreateConnector(_connectionInfo, _socketFactoryMock.Object))
                               .Returns(_connectorMock.Object);
        _ = _connectorMock.Setup(p => p.Connect(_connectionInfo))
                          .Returns(_clientSocket);
        _ = _serviceFactoryMock.Setup(p => p.CreateProtocolVersionExchange())
                               .Returns(_protocolVersionExchangeMock.Object);
        _ = _protocolVersionExchangeMock.Setup(p => p.Start(_session.ClientVersion, _clientSocket, _connectionInfo.Timeout))
                                        .Returns(new SshIdentification("2.0", "XXX"));
    }

    protected void Arrange()
    {
        CreateMocks();
        SetupData();
        SetupMocks();
    }

    protected virtual void Act()
    {
        try
        {
            _session.Connect();
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshConnectionException ex)
        {
            _actualException = ex;
        }
    }

    [Fact]
    public void ConnectShouldThrowSshConnectionException()
    {
        Assert.NotNull(_actualException);
        Assert.Null(_actualException.InnerException);
        Assert.Equal(DisconnectReason.ProtocolError, _actualException.DisconnectReason);
        Assert.Equal("Bad packet length: 168101125.", _actualException.Message);
    }
}
