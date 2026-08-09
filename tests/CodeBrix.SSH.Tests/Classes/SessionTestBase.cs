using CodeBrix.SSH.Connection;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public abstract class SessionTestBase : TripleATestBase
{
    internal Mock<IServiceFactory> ServiceFactoryMock { get; private set; }
    internal Mock<ISocketFactory> SocketFactoryMock { get; private set; }
    internal Mock<IConnector> ConnectorMock { get; private set; }

    protected virtual void CreateMocks()
    {
        ServiceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
        SocketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
        ConnectorMock = new Mock<IConnector>(MockBehavior.Strict);
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
