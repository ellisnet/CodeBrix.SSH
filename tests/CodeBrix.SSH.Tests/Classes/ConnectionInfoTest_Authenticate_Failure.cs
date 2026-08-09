using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Properties;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ConnectionInfoTest_Authenticate_Failure
{
    private Mock<IServiceFactory> _serviceFactoryMock;
    private Mock<IClientAuthentication> _clientAuthenticationMock;
    private Mock<ISession> _sessionMock;
    private ConnectionInfo _connectionInfo;
    private SshAuthenticationException _authenticationException;
    private SshAuthenticationException _actualException;

    public ConnectionInfoTest_Authenticate_Failure()
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
        _authenticationException = new SshAuthenticationException();

        _serviceFactoryMock.Setup(p => p.CreateClientAuthentication()).Returns(_clientAuthenticationMock.Object);
        _clientAuthenticationMock.Setup(p => p.Authenticate(_connectionInfo, _sessionMock.Object))
            .Throws(_authenticationException);
    }

    protected void Act()
    {
        try
        {
            _connectionInfo.Authenticate(_sessionMock.Object, _serviceFactoryMock.Object);
        }
        catch (SshAuthenticationException ex)
        {
            _actualException = ex;
        }
    }

    [Fact]
    public void AuthenticateShouldHaveThrownSshAuthenticationException()
    {
        Assert.NotNull(_actualException);
        Assert.Same(_authenticationException, _actualException);
    }

    [Fact]
    public void IsAuthenticatedShouldReturnFalse()
    {
        Assert.False(_connectionInfo.IsAuthenticated);
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
