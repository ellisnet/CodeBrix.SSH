using System;
using System.Globalization;
using System.Net;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.SSH.Tests.Properties;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Represents remote connection information class.
/// </summary>
public class ConnectionInfoTest : TestBase
{
    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldNotThrowExceptionWhenProxyTypesIsNoneAndProxyHostIsNull()
    {
        const string proxyHost = null;

        var connectionInfo = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME,
            ProxyTypes.None, proxyHost, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
            new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

        Assert.Null(connectionInfo.ProxyHost);
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldThrowArgumentNullExceptionWhenProxyTypesIsNotNoneAndProxyHostIsNull()
    {
        try
        {
            _ = new ConnectionInfo(Resources.HOST,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   ProxyTypes.Http,
                                   null,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   Resources.PASSWORD,
                                   new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("proxyHost", ex.ParamName);
        }
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldNotThrowExceptionWhenProxyTypesIsNotNoneAndProxyHostIsEmpty()
    {
        var proxyHost = string.Empty;

        var connectionInfo = new ConnectionInfo(Resources.HOST,
                                                int.Parse(Resources.PORT),
                                                Resources.USERNAME,
                                                ProxyTypes.Http,
                                                string.Empty,
                                                int.Parse(Resources.PORT),
                                                Resources.USERNAME,
                                                Resources.PASSWORD,
            new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

        Assert.Same(proxyHost, connectionInfo.ProxyHost);
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldThrowArgumentOutOfRangeExceptionWhenProxyTypesIsNotNoneAndProxyPortIsGreaterThanMaximumValue()
    {
        var maxPort = IPEndPoint.MaxPort;

        try
        {
            _ = new ConnectionInfo(Resources.HOST,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   ProxyTypes.Http,
                                   Resources.HOST,
                                   ++maxPort,
                                   Resources.USERNAME,
                                   Resources.PASSWORD,
                                   null);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("proxyPort", ex.ParamName);
        }
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldThrowArgumentOutOfRangeExceptionWhenProxyTypesIsNotNoneAndProxyPortIsLessThanMinimumValue()
    {
        var minPort = IPEndPoint.MinPort;

        try
        {
            _ = new ConnectionInfo(Resources.HOST,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   ProxyTypes.Http,
                                   Resources.HOST,
                                   --minPort,
                                   Resources.USERNAME,
                                   Resources.PASSWORD,
                                   null);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("proxyPort", ex.ParamName);
        }
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void Test_ConnectionInfo_ProxyPort_Valid()
    {
        var proxyPort = new Random().Next(IPEndPoint.MinPort, IPEndPoint.MaxPort);

        var connectionInfo = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME,
            ProxyTypes.None, Resources.HOST, proxyPort, Resources.USERNAME, Resources.PASSWORD,
            new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

        Assert.Equal(proxyPort, connectionInfo.ProxyPort);
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldNotThrowExceptionWhenProxyTypesIsNotNoneAndProxyUsernameIsNull()
    {
        const string proxyUsername = null;

        var connectionInfo = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.Http,
                Resources.PROXY_HOST, int.Parse(Resources.PORT), proxyUsername, Resources.PASSWORD,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

        Assert.Null(connectionInfo.ProxyUsername);
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldThrowArgumentNullExceptionWhenHostIsNull()
    {
        try
        {
            _ = new ConnectionInfo(null,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   ProxyTypes.None,
                                   Resources.HOST,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   Resources.PASSWORD,
                                   null);
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("host", ex.ParamName);
        }
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldNotThrowExceptionWhenHostIsEmpty()
    {
        var host = string.Empty;

        var connectionInfo = new ConnectionInfo(host, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None,
            Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
            new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

        Assert.Same(host, connectionInfo.Host);
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldNotThrowExceptionWhenHostIsInvalidDnsName()
    {
        const string host = "in_valid_host.";

        var connectionInfo = new ConnectionInfo(host, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None,
            Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
            new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

        Assert.Same(host, connectionInfo.Host);
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void Test_ConnectionInfo_Host_Valid()
    {
        var host = new Random().Next().ToString(CultureInfo.InvariantCulture);

        var connectionInfo = new ConnectionInfo(host, int.Parse(Resources.PORT), Resources.USERNAME,
            ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
            new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

        Assert.Same(host, connectionInfo.Host);
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldThrowArgumentOutOfRangeExceptionWhenPortIsGreaterThanMaximumValue()
    {
        const int port = IPEndPoint.MaxPort + 1;

        try
        {
            _ = new ConnectionInfo(Resources.HOST,
                                   port,
                                   Resources.USERNAME,
                                   ProxyTypes.None,
                                   Resources.HOST,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   Resources.PASSWORD,
                                   null);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("port", ex.ParamName);
        }
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldThrowArgumentOutOfRangeExceptionWhenPortIsLessThanMinimumValue()
    {
        const int port = IPEndPoint.MinPort - 1;

        try
        {
            _ = new ConnectionInfo(Resources.HOST,
                                   port,
                                   Resources.USERNAME,
                                   ProxyTypes.None,
                                   Resources.HOST,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   Resources.PASSWORD,
                                   null);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("port", ex.ParamName);
        }
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void Test_ConnectionInfo_Port_Valid()
    {
        var port = new Random().Next(IPEndPoint.MinPort, IPEndPoint.MaxPort);

        var connectionInfo = new ConnectionInfo(Resources.HOST, port, Resources.USERNAME, ProxyTypes.None,
            Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
            new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

        Assert.Equal(port, connectionInfo.Port);
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void Test_ConnectionInfo_Timeout_Valid()
    {
        var connectionInfo = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None,
                                                Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME,
                                                Resources.PASSWORD, new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

        var ex = Assert.ThrowsAny<ArgumentOutOfRangeException>(() => connectionInfo.Timeout = TimeSpan.FromMilliseconds(-2));
        Assert.Equal("Timeout", ex.ParamName);

        ex = Assert.ThrowsAny<ArgumentOutOfRangeException>(() => connectionInfo.Timeout = TimeSpan.FromMilliseconds((double)int.MaxValue + 1));
        Assert.Equal("Timeout", ex.ParamName);

        connectionInfo.Timeout = TimeSpan.FromMilliseconds(-1);
        Assert.Equal(connectionInfo.Timeout, TimeSpan.FromMilliseconds(-1));

        connectionInfo.Timeout = TimeSpan.FromMilliseconds(int.MaxValue);
        Assert.Equal(connectionInfo.Timeout, TimeSpan.FromMilliseconds(int.MaxValue));
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void Test_ConnectionInfo_ChannelCloseTimeout_Valid()
    {
        var connectionInfo = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None,
                                                Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME,
                                                Resources.PASSWORD, new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

        var ex = Assert.ThrowsAny<ArgumentOutOfRangeException>(() => connectionInfo.ChannelCloseTimeout = TimeSpan.FromMilliseconds(-2));
        Assert.Equal("ChannelCloseTimeout", ex.ParamName);

        ex = Assert.ThrowsAny<ArgumentOutOfRangeException>(() => connectionInfo.ChannelCloseTimeout = TimeSpan.FromMilliseconds((double)int.MaxValue + 1));
        Assert.Equal("ChannelCloseTimeout", ex.ParamName);

        connectionInfo.ChannelCloseTimeout = TimeSpan.FromMilliseconds(-1);
        Assert.Equal(connectionInfo.ChannelCloseTimeout, TimeSpan.FromMilliseconds(-1));

        connectionInfo.ChannelCloseTimeout = TimeSpan.FromMilliseconds(int.MaxValue);
        Assert.Equal(connectionInfo.ChannelCloseTimeout, TimeSpan.FromMilliseconds(int.MaxValue));
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldThrowArgumentExceptionWhenUsernameIsNull()
    {
        const string username = null;

        try
        {
            _ = new ConnectionInfo(Resources.HOST,
                                   int.Parse(Resources.PORT),
                                   username,
                                   ProxyTypes.Http,
                                   Resources.USERNAME,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   Resources.PASSWORD,
                                   new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("username", ex.ParamName);
        }
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldThrowArgumentExceptionWhenUsernameIsEmpty()
    {
        var username = string.Empty;

        try
        {
            _ = new ConnectionInfo(Resources.HOST,
                                   int.Parse(Resources.PORT),
                                   username,
                                   ProxyTypes.Http,
                                   Resources.USERNAME,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   Resources.PASSWORD,
                                   new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentException ex)
        {
            Assert.Equal(typeof(ArgumentException), ex.GetType());
            Assert.Null(ex.InnerException);
            Assert.Equal("username", ex.ParamName);
        }
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldThrowArgumentExceptionWhenUsernameContainsOnlyWhitespace()
    {
        const string username = " \t\r";

        try
        {
            _ = new ConnectionInfo(Resources.HOST,
                                   int.Parse(Resources.PORT),
                                   username,
                                   ProxyTypes.Http,
                                   Resources.USERNAME,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   Resources.PASSWORD,
                                   new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentException ex)
        {
            Assert.Equal(typeof(ArgumentException), ex.GetType());
            Assert.Null(ex.InnerException);
            Assert.Equal("username", ex.ParamName);
        }
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldThrowArgumentNullExceptionWhenAuthenticationMethodsIsNull()
    {
        try
        {
            _ = new ConnectionInfo(Resources.HOST,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   ProxyTypes.None,
                                   Resources.HOST,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   Resources.PASSWORD,
                                   null);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("authenticationMethods", ex.ParamName);
        }
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void ConstructorShouldThrowArgumentNullExceptionWhenAuthenticationMethodsIsZeroLength()
    {
        try
        {
            _ = new ConnectionInfo(Resources.HOST,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   ProxyTypes.None,
                                   Resources.HOST,
                                   int.Parse(Resources.PORT),
                                   Resources.USERNAME,
                                   Resources.PASSWORD,
                                   new AuthenticationMethod[0]);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("authenticationMethods", ex.ParamName);
        }
    }

    [Fact]
    [Trait("Category", "ConnectionInfo")]
    public void AuthenticateShouldThrowArgumentNullExceptionWhenServiceFactoryIsNull()
    {
        var connectionInfo = new ConnectionInfo(Resources.HOST,
                                                int.Parse(Resources.PORT),
                                                Resources.USERNAME,
                                                ProxyTypes.None,
                                                Resources.HOST,
                                                int.Parse(Resources.PORT),
                                                Resources.USERNAME,
                                                Resources.PASSWORD,
                                                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

        var session = new Mock<ISession>(MockBehavior.Strict).Object;
        const IServiceFactory serviceFactory = null;

        try
        {
            connectionInfo.Authenticate(session, serviceFactory);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("serviceFactory", ex.ParamName);
        }
    }
}
