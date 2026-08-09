using CodeBrix.SSH.Tests.Properties;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ConnectionInfoTest_Authenticate_Success
{
    private Mock<IServiceFactory> _serviceFactoryMock;
    private Mock<IClientAuthentication> _clientAuthenticationMock;
    private Mock<ISession> _sessionMock;
    private ConnectionInfo _connectionInfo;

    public ConnectionInfoTest_Authenticate_Success()
    {
        Init();
    }

    private void Init()
    {
        Arrange();
        Act();
    }

    protected void Arrange()
    {
        _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
        _clientAuthenticationMock = new Mock<IClientAuthentication>(MockBehavior.Strict);
        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);

        _connectionInfo = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None,
            Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
            new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

        _serviceFactoryMock.Setup(p => p.CreateClientAuthentication()).Returns(_clientAuthenticationMock.Object);
        _clientAuthenticationMock.Setup(p => p.Authenticate(_connectionInfo, _sessionMock.Object));
    }

    protected void Act()
    {
        _connectionInfo.Authenticate(_sessionMock.Object, _serviceFactoryMock.Object);
    }

    [Fact]
    public void IsAuthenticatedShouldReturnTrue()
    {
        Assert.True(_connectionInfo.IsAuthenticated);
    }

    [Fact]
    public void CreateClientAuthenticationOnServiceFactoryShouldBeInvokedOnce()
    {
        _serviceFactoryMock.Verify(p => p.CreateClientAuthentication(), Times.Once);
    }

    [Fact]
    public void AuthenticateOnClientAuthenticationShouldBeInvokedOnce()
    {
        _clientAuthenticationMock.Verify(p => p.Authenticate(_connectionInfo, _sessionMock.Object), Times.Once);
    }
}
