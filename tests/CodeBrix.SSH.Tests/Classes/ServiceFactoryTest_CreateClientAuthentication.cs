using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ServiceFactoryTest_CreateClientAuthentication
{
    private ServiceFactory _serviceFactory;
    private IClientAuthentication _actual;

    private void Arrange()
    {
        _serviceFactory = new ServiceFactory();
    }

    public ServiceFactoryTest_CreateClientAuthentication()
    {
        Initialize();
    }

    private void Initialize()
    {
        Arrange();
        Act();
    }

    private void Act()
    {
        _actual = _serviceFactory.CreateClientAuthentication();
    }

    [Fact]
    public void CreateClientAuthenticationShouldNotReturnNull()
    {
        Assert.NotNull(_actual);
    }

    [Fact]
    public void ClientAuthenticationShouldHavePartialSuccessLimitOf5()
    {
        var clientAuthentication = _actual as ClientAuthentication;
        Assert.NotNull(clientAuthentication);
        Assert.Equal(5, clientAuthentication.PartialSuccessLimit);
    }
}
