using System;
using System.Threading;
using CodeBrix.SSH.Messages.Transport;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class BaseClientTest_Connected_KeepAliveInterval_NegativeOne : BaseClientTestBase
{
    private BaseClient _client;
    private ConnectionInfo _connectionInfo;
    private TimeSpan _keepAliveInterval;
    private int _keepAliveCount;

    protected override void SetupData()
    {
        _connectionInfo = new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "pwd"));
        _keepAliveInterval = TimeSpan.FromMilliseconds(100d);
        _keepAliveCount = 0;
    }

    protected override void SetupMocks()
    {
        _ = ServiceFactoryMock.Setup(p => p.CreateSocketFactory())
                               .Returns(SocketFactoryMock.Object);
        _ = ServiceFactoryMock.Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                               .Returns(SessionMock.Object);
        _ = SessionMock.Setup(p => p.Connect());
        _ = SessionMock.Setup(p => p.IsConnected)
                        .Returns(true);
        _ = SessionMock.Setup(p => p.TrySendMessage(It.IsAny<IgnoreMessage>()))
                        .Returns(true)
                        .Callback(() => Interlocked.Increment(ref _keepAliveCount));
    }

    protected override void Arrange()
    {
        base.Arrange();

        _client = new MyClient(_connectionInfo, false, ServiceFactoryMock.Object);
        _client.Connect();
        _client.KeepAliveInterval = _keepAliveInterval;
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
        // allow keep-alive to be sent once
        Thread.Sleep(150);

        // disable keep-alive
        _client.KeepAliveInterval = TimeSpan.FromMilliseconds(-1);
    }

    [Fact]
    public void KeepAliveIntervalShouldReturnConfiguredValue()
    {
        Assert.Equal(TimeSpan.FromMilliseconds(-1), _client.KeepAliveInterval);
    }

    [Fact]
    public void CreateSocketFactoryOnServiceFactoryShouldBeInvokedOnce()
    {
        ServiceFactoryMock.Verify(p => p.CreateSocketFactory(), Times.Once);
    }

    [Fact]
    public void CreateSessionOnServiceFactoryShouldBeInvokedOnce()
    {
        ServiceFactoryMock.Verify(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object), Times.Once);
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
    public void SendMessageOnSessionShouldBeInvokedOneTime()
    {
        // allow keep-alive to be sent once
        Thread.Sleep(100);

        SessionMock.Verify(p => p.TrySendMessage(It.IsAny<IgnoreMessage>()), Times.Exactly(1));
    }

    private class MyClient : BaseClient
    {
        public MyClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory) : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
        }
    }
}
