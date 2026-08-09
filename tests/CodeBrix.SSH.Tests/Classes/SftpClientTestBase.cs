using CodeBrix.SSH.Sftp;
using CodeBrix.TestMocks.Mocking;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public abstract class SftpClientTestBase : BaseClientTestBase
{
    internal Mock<ISftpResponseFactory> SftpResponseFactoryMock { get; private set; }
    internal Mock<ISftpSession> SftpSessionMock { get; private set; }

    protected override void CreateMocks()
    {
        base.CreateMocks();

        SftpResponseFactoryMock = new Mock<ISftpResponseFactory>(MockBehavior.Strict);
        SftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);
    }
}
