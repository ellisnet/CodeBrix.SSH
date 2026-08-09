using CodeBrix.SSH.Connection;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public abstract class BaseClientTestBase : TripleATestBase
{
    internal Mock<IServiceFactory> ServiceFactoryMock { get; private set; }
    internal Mock<ISocketFactory> SocketFactoryMock { get; private set; }
    internal Mock<ISession> SessionMock { get; private set; }

    protected virtual void CreateMocks()
    {
        ServiceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
        SocketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
        SessionMock = new Mock<ISession>(MockBehavior.Strict);
        SessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
    }

    protected virtual void SetupData()
    {
    }

    protected virtual void SetupMocks()
    {
    }

    protected override void Arrange()
    {
        CreateMocks();
        SetupData();
        SetupMocks();
    }
}
