using System;
using System.Net;
using CodeBrix.SSH.Connection;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Provides functionality to connect and interact with SSH server.
/// </summary>
public partial class SessionTest : TestBase
{
    private Mock<IServiceFactory> _serviceFactoryMock;
    private Mock<ISocketFactory> _socketFactoryMock;

    protected override void OnInit()
    {
        base.OnInit();

        _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
        _socketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
    }

    [Fact]
    public void ConstructorShouldThrowArgumentNullExceptionWhenConnectionInfoIsNull()
    {
        const ConnectionInfo connectionInfo = null;

        try
        {
            _ = new Session(connectionInfo, _serviceFactoryMock.Object, _socketFactoryMock.Object);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("connectionInfo", ex.ParamName);
        }
    }

    [Fact]
    public void ConstructorShouldThrowArgumentNullExceptionWhenServiceFactoryIsNull()
    {
        var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
        var connectionInfo = CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5));
        IServiceFactory serviceFactory = null;

        try
        {
            _ = new Session(connectionInfo, serviceFactory, _socketFactoryMock.Object);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("serviceFactory", ex.ParamName);
        }
    }

    [Fact]
    public void ConstructorShouldThrowArgumentNullExceptionWhenSocketFactoryIsNull()
    {
        var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
        var connectionInfo = CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5));
        const ISocketFactory socketFactory = null;

        try
        {
            _ = new Session(connectionInfo, _serviceFactoryMock.Object, socketFactory);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("socketFactory", ex.ParamName);
        }
    }

    private static ConnectionInfo CreateConnectionInfo(IPEndPoint serverEndPoint, TimeSpan timeout)
    {
        return new ConnectionInfo(serverEndPoint.Address.ToString(), serverEndPoint.Port, "eric", new NoneAuthenticationMethod("eric"))
        {
            Timeout = timeout
        };
    }
}
