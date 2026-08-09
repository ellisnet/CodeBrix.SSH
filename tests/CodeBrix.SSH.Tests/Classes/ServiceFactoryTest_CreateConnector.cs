using System;
using CodeBrix.SSH.Connection;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ServiceFactoryTest_CreateConnector
{
    private ServiceFactory _serviceFactory;
    private Mock<IConnectionInfo> _connectionInfoMock;
    private Mock<ISocketFactory> _socketFactoryMock;

    public ServiceFactoryTest_CreateConnector()
    {
        Setup();
    }

    private void Setup()
    {
        _serviceFactory = new ServiceFactory();
        _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
        _connectionInfoMock.Setup(p => p.LoggerFactory).Returns(NullLoggerFactory.Instance);
        _socketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
    }

    [Fact]
    public void ConnectionInfoIsNull()
    {
        const IConnectionInfo connectionInfo = null;

        try
        {
            _serviceFactory.CreateConnector(connectionInfo, _socketFactoryMock.Object);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("connectionInfo", ex.ParamName);
        }
    }

    [Fact]
    public void SocketFactoryIsNull()
    {
        const ISocketFactory socketFactory = null;

        try
        {
            _serviceFactory.CreateConnector(_connectionInfoMock.Object, socketFactory);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("socketFactory", ex.ParamName);
        }
    }

    [Fact]
    public void ProxyType_Http()
    {
        _connectionInfoMock.Setup(p => p.ProxyType).Returns(ProxyTypes.Http);

        var actual = _serviceFactory.CreateConnector(_connectionInfoMock.Object, _socketFactoryMock.Object);

        Assert.NotNull(actual);
        Assert.Equal(typeof(HttpConnector), actual.GetType());

        var httpConnector = (HttpConnector)actual;
        Assert.Same(_socketFactoryMock.Object, httpConnector.SocketFactory);

        _connectionInfoMock.Verify(p => p.ProxyType, Times.Once);
    }

    [Fact]
    public void ProxyType_None()
    {
        _connectionInfoMock.Setup(p => p.ProxyType).Returns(ProxyTypes.None);

        var actual = _serviceFactory.CreateConnector(_connectionInfoMock.Object, _socketFactoryMock.Object);

        Assert.NotNull(actual);
        Assert.Equal(typeof(DirectConnector), actual.GetType());

        var directConnector = (DirectConnector)actual;
        Assert.Same(_socketFactoryMock.Object, directConnector.SocketFactory);

        _connectionInfoMock.Verify(p => p.ProxyType, Times.Once);
    }

    [Fact]
    public void ProxyType_Socks4()
    {
        _connectionInfoMock.Setup(p => p.ProxyType).Returns(ProxyTypes.Socks4);

        var actual = _serviceFactory.CreateConnector(_connectionInfoMock.Object, _socketFactoryMock.Object);

        Assert.NotNull(actual);
        Assert.Equal(typeof(Socks4Connector), actual.GetType());

        var socks4Connector = (Socks4Connector)actual;
        Assert.Same(_socketFactoryMock.Object, socks4Connector.SocketFactory);

        _connectionInfoMock.Verify(p => p.ProxyType, Times.Once);
    }

    [Fact]
    public void ProxyType_Socks5()
    {
        _connectionInfoMock.Setup(p => p.ProxyType).Returns(ProxyTypes.Socks5);

        var actual = _serviceFactory.CreateConnector(_connectionInfoMock.Object, _socketFactoryMock.Object);

        Assert.NotNull(actual);
        Assert.Equal(typeof(Socks5Connector), actual.GetType());

        var socks5Connector = (Socks5Connector)actual;
        Assert.Same(_socketFactoryMock.Object, socks5Connector.SocketFactory);

        _connectionInfoMock.Verify(p => p.ProxyType, Times.Once);
    }

    [Fact]
    public void ProxyType_Undefined()
    {
        _connectionInfoMock.Setup(p => p.ProxyType).Returns((ProxyTypes)666);

        try
        {
            _serviceFactory.CreateConnector(_connectionInfoMock.Object, _socketFactoryMock.Object);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (NotSupportedException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("ProxyTypes '666' is not supported.", ex.Message);
        }

        _connectionInfoMock.Verify(p => p.ProxyType, Times.Exactly(2));
    }
}
