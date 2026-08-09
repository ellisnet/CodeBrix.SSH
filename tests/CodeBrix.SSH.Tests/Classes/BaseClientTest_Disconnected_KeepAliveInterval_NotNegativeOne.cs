using System;
using System.Threading;
using CodeBrix.SSH.Messages.Transport;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class BaseClientTest_Disconnected_KeepAliveInterval_NotNegativeOne : BaseClientTestBase
{
    private BaseClient _client;
    private ConnectionInfo _connectionInfo;
    private TimeSpan _keepAliveInterval;

    protected override void SetupData()
    {
        _connectionInfo = new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "pwd"));
        _keepAliveInterval = TimeSpan.FromMilliseconds(50d);
    }

    protected override void SetupMocks()
    {
        ServiceFactoryMock.Setup(p => p.CreateSocketFactory())
                           .Returns(SocketFactoryMock.Object);
        ServiceFactoryMock.Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                           .Returns(SessionMock.Object);
        SessionMock.Setup(p => p.Connect());
        SessionMock.Setup(p => p.IsConnected).Returns(false);
        SessionMock.Setup(p => p.TrySendMessage(It.IsAny<IgnoreMessage>()))
                    .Returns(true);
    }

    protected override void Arrange()
    {
        base.Arrange();

        _client = new MyClient(_connectionInfo, false, ServiceFactoryMock.Object);
        _client.Connect();
    }

    protected override void TearDown()
    {
        if (_client != null)
        {
            SessionMock.Setup(p => p.OnDisconnecting());
            SessionMock.Setup(p => p.Dispose());
            _client.Dispose();
        }
    }

    protected override void Act()
    {
        _client.KeepAliveInterval = _keepAliveInterval;

        // allow keep-alive to be sent a few times
        Thread.Sleep(195);
    }

    [Fact]
    public void KeepAliveIntervalShouldReturnConfiguredValue()
    {
        Assert.Equal(_keepAliveInterval, _client.KeepAliveInterval);
    }

    [Fact]
    public void CreateSocketFactoryOnServiceFactoryShouldBeInvokedOnce()
    {
        ServiceFactoryMock.Verify(p => p.CreateSocketFactory(), Times.Once);
    }

    [Fact]
    public void CreateSessionOnServiceFactoryShouldBeInvokedOnce()
    {
        ServiceFactoryMock.Verify(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object),
                                   Times.Once);
    }

    [Fact]
    public void ConnectOnSessionShouldBeInvokedOnce()
    {
        SessionMock.Verify(p => p.Connect(), Times.Once);
    }

    [Fact]
    public void IsConnectedOnSessionShouldBeInvokedOnce()
    {
        SessionMock.Verify(p => p.IsConnected, Times.Once);
    }

    [Fact]
    public void SendMessageOnSessionShouldNeverBeInvoked()
    {
        SessionMock.Verify(p => p.TrySendMessage(It.IsAny<IgnoreMessage>()), Times.Never);
    }

    private class MyClient : BaseClient
    {
        public MyClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory) : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
        }
    }
}
