using System;
using System.Linq;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Security;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class NetConfClientTest_Connect_NetConfSessionConnectFailure : NetConfClientTestBase
{
    private ConnectionInfo _connectionInfo;
    private ApplicationException _netConfSessionConnectionException;
    private NetConfClient _netConfClient;
    private ApplicationException _actualException;

    protected override void SetupData()
    {
        _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
        _netConfSessionConnectionException = new ApplicationException();
        _netConfClient = new NetConfClient(_connectionInfo, false, ServiceFactoryMock.Object);
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
                               .Setup(p => p.CreateNetConfSession(SessionMock.Object, -1))
                               .Returns(NetConfSessionMock.Object);
        _ = NetConfSessionMock.InSequence(sequence)
                               .Setup(p => p.Connect())
                               .Throws(_netConfSessionConnectionException);
        _ = NetConfSessionMock.InSequence(sequence)
                               .Setup(p => p.Dispose());
        _ = SessionMock.InSequence(sequence)
                        .Setup(p => p.Dispose());
    }

    protected override void Act()
    {
        try
        {
            _netConfClient.Connect();
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ApplicationException ex)
        {
            _actualException = ex;
        }
    }

    [Fact]
    public void ConnectShouldHaveThrownApplicationException()
    {
        Assert.NotNull(_actualException);
        Assert.Same(_netConfSessionConnectionException, _actualException);
    }

    [Fact]
    public void SessionShouldBeNull()
    {
        Assert.Null(_netConfClient.Session);
    }

    [Fact]
    public void NetConfSessionShouldBeNull()
    {
        Assert.Null(_netConfClient.NetConfSession);
    }

    [Fact]
    public void ErrorOccurredOnSessionShouldNoLongerBeSignaledViaErrorOccurredOnNetConfClient()
    {
        var errorOccurredSignalCount = 0;

        _netConfClient.ErrorOccurred += (sender, args) => Interlocked.Increment(ref errorOccurredSignalCount);

        SessionMock.Raise(p => p.ErrorOccured += null, new ExceptionEventArgs(new Exception()));

        Assert.Equal(0, errorOccurredSignalCount);
    }

    [Fact]
    public void HostKeyReceivedOnSessionShouldNoLongerBeSignaledViaHostKeyReceivedOnSftpClient()
    {
        var hostKeyReceivedSignalCount = 0;

        _netConfClient.HostKeyReceived += (sender, args) => Interlocked.Increment(ref hostKeyReceivedSignalCount);

        SessionMock.Raise(p => p.HostKeyReceived += null, new HostKeyEventArgs(GetKeyHostAlgorithm()));

        Assert.Equal(0, hostKeyReceivedSignalCount);
    }

    private static KeyHostAlgorithm GetKeyHostAlgorithm()
    {
        using (var s = TestBase.GetData("Key.RSA.txt"))
        {
            var privateKey = new PrivateKeyFile(s);
            return (KeyHostAlgorithm)privateKey.HostKeyAlgorithms.First();
        }
    }
}
