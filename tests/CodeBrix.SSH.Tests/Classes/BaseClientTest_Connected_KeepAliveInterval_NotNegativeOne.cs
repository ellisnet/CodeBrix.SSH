using System;
using System.Threading;
using CodeBrix.SSH.Messages.Transport;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class BaseClientTest_Connected_KeepAliveInterval_NotNegativeOne : BaseClientTestBase
{
    private BaseClient _client;
    private ConnectionInfo _connectionInfo;
    private TimeSpan _keepAliveInterval;
    private int _keepAliveCount;

    protected override void SetupData()
    {
        _connectionInfo = new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "pwd"));
        _keepAliveInterval = TimeSpan.FromMilliseconds(50d);
        _keepAliveCount = 0;
    }

    protected override void SetupMocks()
    {
        ServiceFactoryMock.Setup(p => p.CreateSocketFactory())
                           .Returns(SocketFactoryMock.Object);
        ServiceFactoryMock.Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                           .Returns(SessionMock.Object);
        SessionMock.Setup(p => p.Connect());
        SessionMock.Setup(p => p.IsConnected).Returns(true);
        SessionMock.Setup(p => p.TrySendMessage(It.IsAny<IgnoreMessage>()))
                    .Returns(true)
                    .Callback(() => Interlocked.Increment(ref _keepAliveCount));
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

        Thread.Sleep(200);
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
    public void SendMessageOnSessionShouldBeInvokedThreeTimes()
    {
#pragma warning disable IDE0002 // Name can be simplified; "Ambiguous reference between CodeBrix.TestMocks.Mocking.Range and System.Range"
        SessionMock.Verify(p => p.TrySendMessage(It.IsAny<IgnoreMessage>()), Times.Between(2, 4, CodeBrix.TestMocks.Mocking.Range.Inclusive));
#pragma warning restore IDE0002
    }

    private class MyClient : BaseClient
    {
        public MyClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory) : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
        }
    }
}
