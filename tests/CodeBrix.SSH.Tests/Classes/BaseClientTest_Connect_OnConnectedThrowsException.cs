using System;
using System.Linq;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Security;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class BaseClientTest_Connect_OnConnectedThrowsException : BaseClientTestBase
{
    private MyClient _client;
    private ConnectionInfo _connectionInfo;
    private ApplicationException _onConnectException;
    private ApplicationException _actualException;

    protected override void SetupData()
    {
        _connectionInfo = new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "pwd"));
        _onConnectException = new ApplicationException();
    }

    protected override void SetupMocks()
    {
        ServiceFactoryMock.Setup(p => p.CreateSocketFactory())
                           .Returns(SocketFactoryMock.Object);
        ServiceFactoryMock.Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                           .Returns(SessionMock.Object);
        SessionMock.Setup(p => p.Connect());
        SessionMock.Setup(p => p.Dispose());
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

    protected override void Arrange()
    {
        base.Arrange();

        _client = new MyClient(_connectionInfo, false, ServiceFactoryMock.Object)
        {
            OnConnectedException = _onConnectException
        };
    }

    protected override void Act()
    {
        try
        {
            _client.Connect();
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ApplicationException ex)
        {
            _actualException = ex;
        }
    }

    [Fact]
    public void ConnectShouldRethrowExceptionThrownByOnConnect()
    {
        Assert.NotNull(_actualException);
        Assert.Same(_onConnectException, _actualException);
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
    public void DisposeOnSessionShouldBeInvokedOnce()
    {
        SessionMock.Verify(p => p.Dispose(), Times.Once);
    }

    [Fact]
    public void ErrorOccurredOnSessionShouldNoLongerBeSignaledViaErrorOccurredOnBaseClient()
    {
        var errorOccurredSignalCount = 0;

        _client.ErrorOccurred += (sender, args) => Interlocked.Increment(ref errorOccurredSignalCount);

        SessionMock.Raise(p => p.ErrorOccured += null, new ExceptionEventArgs(new Exception()));

        Assert.Equal(0, errorOccurredSignalCount);
    }

    [Fact]
    public void HostKeyReceivedOnSessionShouldNoLongerBeSignaledViaHostKeyReceivedOnBaseClient()
    {
        var hostKeyReceivedSignalCount = 0;

        _client.HostKeyReceived += (sender, args) => Interlocked.Increment(ref hostKeyReceivedSignalCount);

        SessionMock.Raise(p => p.HostKeyReceived += null, new HostKeyEventArgs(GetKeyHostAlgorithm()));

        Assert.Equal(0, hostKeyReceivedSignalCount);
    }

    [Fact]
    public void SessionShouldBeNull()
    {
        Assert.Null(_client.Session);
    }

    [Fact]
    public void IsConnectedShouldReturnFalse()
    {
        Assert.False(_client.IsConnected);
    }

    private static KeyHostAlgorithm GetKeyHostAlgorithm()
    {
        using (var s = TestBase.GetData("Key.RSA.txt"))
        {
            var privateKey = new PrivateKeyFile(s);
            return (KeyHostAlgorithm)privateKey.HostKeyAlgorithms.First();
        }
    }

    private class MyClient : BaseClient
    {
        private int _onConnectedCount;

        public MyClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory) : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
        }

        public Exception OnConnectedException { get; set; }

        protected override void OnConnected()
        {
            base.OnConnected();

            Interlocked.Increment(ref _onConnectedCount);

            if (OnConnectedException != null)
            {
                throw OnConnectedException;
            }
        }
    }

}
