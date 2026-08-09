using CodeBrix.SSH.Connection;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeBrix.SSH.Tests.Classes.Connection; //was previously: Renci.SshNet.Tests.Classes.Connection;

public abstract class DirectConnectorTestBase : TripleATestBase
{
    internal Mock<ISocketFactory> SocketFactoryMock { get; private set; }
    internal DirectConnector Connector { get; private set; }
    internal SocketFactory SocketFactory { get; private set; }

    protected virtual void CreateMocks()
    {
        SocketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
    }

    protected virtual void SetupData()
    {
        Connector = new DirectConnector(SocketFactoryMock.Object, NullLoggerFactory.Instance);
        SocketFactory = new SocketFactory();
    }

    protected virtual void SetupMocks()
    {
    }

    protected sealed override void Arrange()
    {
        CreateMocks();
        SetupData();
        SetupMocks();
    }

    protected ConnectionInfo CreateConnectionInfo(string hostName)
    {
        return new ConnectionInfo(hostName,
                                  1027,
                                  "user",
                                  new KeyboardInteractiveAuthenticationMethod("user"));
    }
}
