using System;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SftpClientTest_Finalize_Connected : SftpClientTestBase
{
    private SftpClient _sftpClient;
    private ConnectionInfo _connectionInfo;
    private int _operationTimeout;
    private WeakReference _sftpClientWeakRefence;

    protected override void SetupData()
    {
        _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
        _operationTimeout = new Random().Next(1000, 10000);
        _sftpClient = new SftpClient(_connectionInfo, false, ServiceFactoryMock.Object)
        {
            OperationTimeout = TimeSpan.FromMilliseconds(_operationTimeout)
        };
        _sftpClientWeakRefence = new WeakReference(_sftpClient);
    }

    protected override void SetupMocks()
    {
        _ = ServiceFactoryMock.Setup(p => p.CreateSocketFactory())
                               .Returns(SocketFactoryMock.Object);
        _ = ServiceFactoryMock.Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                               .Returns(SessionMock.Object);
        _ = SessionMock.Setup(p => p.Connect());
        _ = ServiceFactoryMock.Setup(p => p.CreateSftpResponseFactory())
                               .Returns(SftpResponseFactoryMock.Object);
        _ = ServiceFactoryMock.Setup(p => p.CreateSftpSession(SessionMock.Object, _operationTimeout, _connectionInfo.Encoding, SftpResponseFactoryMock.Object))
                               .Returns(SftpSessionMock.Object);
        _ = SftpSessionMock.Setup(p => p.Connect());
    }

    protected override void Arrange()
    {
        base.Arrange();

        _sftpClient.Connect();
        _sftpClient = null;

        // We need to dereference all mocks as they might otherwise hold the target alive
        //(through recorded invocations?)
        CreateMocks();
    }

    protected override void Act()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    [Fact]
    public void DisconnectOnSftpSessionShouldNeverBeInvoked()
    {
        // Since we recreated the mocks, this test has no value
        // We'll leaving ths test just in case we have a solution that does not require us
        // to recreate the mocks
        SftpSessionMock.Verify(p => p.Disconnect(), Times.Never);
    }

    [Fact]
    public void DisposeOnSftpSessionShouldNeverBeInvoked()
    {
        // Since we recreated the mocks, this test has no value
        // We'll leaving ths test just in case we have a solution that does not require us
        // to recreate the mocks
        SftpSessionMock.Verify(p => p.Dispose(), Times.Never);
    }

    [Fact]
    public void SftpClientShouldHaveBeenFinalized()
    {
        Assert.Null(_sftpClientWeakRefence.Target);
    }
}
