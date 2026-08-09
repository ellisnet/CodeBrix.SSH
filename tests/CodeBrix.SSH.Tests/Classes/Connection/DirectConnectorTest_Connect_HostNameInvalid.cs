using System.Net.Sockets;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Connection; //was previously: Renci.SshNet.Tests.Classes.Connection;

public class DirectConnectorTest_Connect_HostNameInvalid : DirectConnectorTestBase
{
    private ConnectionInfo _connectionInfo;
    private Socket _clientSocket;
    private SocketException _actualException;

    protected override void SetupData()
    {
        base.SetupData();

        _connectionInfo = CreateConnectionInfo("invalid.");
        _actualException = null;
        _clientSocket = SocketFactory.Create(SocketType.Stream, ProtocolType.Tcp);
    }

    protected override void Act()
    {
        try
        {
            _ = Connector.Connect(_connectionInfo);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SocketException ex)
        {
            _actualException = ex;
        }
    }

    protected override void SetupMocks()
    {
        _ = SocketFactoryMock.Setup(p => p.Create(SocketType.Stream, ProtocolType.Tcp))
                             .Returns(_clientSocket);
    }

    [Fact]
    public void ConnectShouldHaveThrownSocketException()
    {
        Assert.NotNull(_actualException);
        Assert.Null(_actualException.InnerException);
        Assert.True(_actualException.SocketErrorCode is SocketError.HostNotFound or SocketError.TryAgain or SocketError.NoData);
    }
}
