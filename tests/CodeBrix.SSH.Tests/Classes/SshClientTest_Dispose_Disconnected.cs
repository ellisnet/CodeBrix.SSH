using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SshClientTest_Dispose_Disconnected : BaseClientTestBase
{
    private SshClient _sshClient;
    private ConnectionInfo _connectionInfo;

    protected override void SetupData()
    {
        _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
    }

    protected override void SetupMocks()
    {
        var sequence = new MockSequence();

        ServiceFactoryMock.InSequence(sequence)
                           .Setup(p => p.CreateSocketFactory())
                           .Returns(SocketFactoryMock.Object);
        ServiceFactoryMock.InSequence(sequence)
                           .Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                           .Returns(SessionMock.Object);
        SessionMock.InSequence(sequence).Setup(p => p.Connect());
        SessionMock.InSequence(sequence).Setup(p => p.OnDisconnecting());
        SessionMock.InSequence(sequence).Setup(p => p.Dispose());
    }

    protected override void Arrange()
    {
        base.Arrange();

        _sshClient = new SshClient(_connectionInfo, false, ServiceFactoryMock.Object);
        _sshClient.Connect();
        _sshClient.Disconnect();
    }

    protected override void Act()
    {
        _sshClient.Dispose();
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
    public void DisconnectOnSessionShouldNeverBeInvoked()
    {
        SessionMock.Verify(p => p.Disconnect(), Times.Never);
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
