using System;
using System.Threading.Tasks;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class NetConfClientTest_Dispose_Disconnected : NetConfClientTestBase
{
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        Setup();
    }

    private NetConfClient _netConfClient;
    private ConnectionInfo _connectionInfo;
    private int _operationTimeout;

    private void Setup()
    {
        Arrange();
        Act();
    }

    protected override void SetupData()
    {
        _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
        _operationTimeout = new Random().Next(1000, 10000);
        _netConfClient = new NetConfClient(_connectionInfo, false, ServiceFactoryMock.Object)
        {
            OperationTimeout = TimeSpan.FromMilliseconds(_operationTimeout)
        };
    }

    protected override void SetupMocks()
    {
        var sequence = new MockSequence();

        _ = ServiceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSocketFactory())
                               .Returns(SocketFactoryMock.Object);
        _ = ServiceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                               .Returns(SessionMock.Object);
        _ = SessionMock.InSequence(sequence)
                        .Setup(p => p.Connect());
        _ = ServiceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateNetConfSession(SessionMock.Object, _operationTimeout))
                               .Returns(NetConfSessionMock.Object);
        _ = NetConfSessionMock.InSequence(sequence)
                               .Setup(p => p.Connect());
        _ = SessionMock.InSequence(sequence)
                        .Setup(p => p.OnDisconnecting());
        _ = NetConfSessionMock.InSequence(sequence)
                               .Setup(p => p.Disconnect());
        _ = SessionMock.InSequence(sequence)
                        .Setup(p => p.Dispose());
        _ = NetConfSessionMock.InSequence(sequence)
                               .Setup(p => p.Disconnect());
        _ = NetConfSessionMock.InSequence(sequence)
                               .Setup(p => p.Dispose());
    }

    protected override void Arrange()
    {
        base.Arrange();

        _netConfClient.Connect();
        _netConfClient.Disconnect();
    }

    protected override void Act()
    {
        _netConfClient.Dispose();
    }

    [Fact]
    public void CreateNetConfSessionOnServiceFactoryShouldBeInvokedOnce()
    {
        ServiceFactoryMock.Verify(p => p.CreateNetConfSession(SessionMock.Object, _operationTimeout), Times.Once);
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
    public void DisconnectOnNetConfSessionShouldBeInvokedTwice()
    {
        NetConfSessionMock.Verify(p => p.Disconnect(), Times.Exactly(2));
    }

    [Fact]
    public void DisconnectOnSessionShouldNeverBeInvoked()
    {
        SessionMock.Verify(p => p.Disconnect(), Times.Never);
    }

    [Fact]
    public void DisposeOnNetConfSessionShouldBeInvokedOnce()
    {
        NetConfSessionMock.Verify(p => p.Dispose(), Times.Once);
    }

    [Fact]
    public void DisposeOnSessionShouldBeInvokedOnce()
    {
        SessionMock.Verify(p => p.Dispose(), Times.Once);
    }

    [Fact]
    public void OnDisconnectingOnSessionShouldBeInvokedOnce()
    {
        SessionMock.Verify(p => p.OnDisconnecting(), Times.Once);
    }
}
