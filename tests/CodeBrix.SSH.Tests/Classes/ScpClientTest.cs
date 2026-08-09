using System;
using System.Text;
using System.Threading.Tasks;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Provides SCP client functionality.
/// </summary>
public partial class ScpClientTest : TestBase
{
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        SetUp();
    }

    private Random _random;

    private void SetUp()
    {
        _random = new Random();
    }

    [Fact]
    public void Ctor_ConnectionInfo_Null()
    {
        const ConnectionInfo connectionInfo = null;

        try
        {
            _ = new ScpClient(connectionInfo);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("connectionInfo", ex.ParamName);
        }
    }

    [Fact]
    public void Ctor_ConnectionInfo_NotNull()
    {
        var connectionInfo = new ConnectionInfo("HOST", "USER", new PasswordAuthenticationMethod("USER", "PWD"));

        var client = new ScpClient(connectionInfo);
        Assert.Equal(16 * 1024U, client.BufferSize);
        Assert.Same(connectionInfo, client.ConnectionInfo);
        Assert.False(client.IsConnected);
        Assert.Equal(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
        Assert.Equal(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
        Assert.Same(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
        Assert.Null(client.Session);
    }

    [Fact]
    public void Ctor_HostAndPortAndUsernameAndPassword()
    {
        var host = _random.Next().ToString();
        var port = _random.Next(1, 100);
        var userName = _random.Next().ToString();
        var password = _random.Next().ToString();

        var client = new ScpClient(host, port, userName, password);
        Assert.Equal(16 * 1024U, client.BufferSize);
        Assert.NotNull(client.ConnectionInfo);
        Assert.False(client.IsConnected);
        Assert.Equal(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
        Assert.Equal(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
        Assert.Same(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
        Assert.Null(client.Session);

        var passwordConnectionInfo = client.ConnectionInfo as PasswordConnectionInfo;
        Assert.NotNull(passwordConnectionInfo);
        Assert.Equal(host, passwordConnectionInfo.Host);
        Assert.Equal(port, passwordConnectionInfo.Port);
        Assert.Same(userName, passwordConnectionInfo.Username);
        Assert.NotNull(passwordConnectionInfo.AuthenticationMethods);
        Assert.Single(passwordConnectionInfo.AuthenticationMethods);

        var passwordAuthentication = passwordConnectionInfo.AuthenticationMethods[0] as PasswordAuthenticationMethod;
        Assert.NotNull(passwordAuthentication);
        Assert.Equal(userName, passwordAuthentication.Username);
        Assert.True(Encoding.UTF8.GetBytes(password).IsEqualTo(passwordAuthentication.Password));
    }

    [Fact]
    public void Ctor_HostAndUsernameAndPassword()
    {
        var host = _random.Next().ToString();
        var userName = _random.Next().ToString();
        var password = _random.Next().ToString();

        var client = new ScpClient(host, userName, password);
        Assert.Equal(16 * 1024U, client.BufferSize);
        Assert.NotNull(client.ConnectionInfo);
        Assert.False(client.IsConnected);
        Assert.Equal(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
        Assert.Equal(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
        Assert.Same(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
        Assert.Null(client.Session);

        var passwordConnectionInfo = client.ConnectionInfo as PasswordConnectionInfo;
        Assert.NotNull(passwordConnectionInfo);
        Assert.Equal(host, passwordConnectionInfo.Host);
        Assert.Equal(22, passwordConnectionInfo.Port);
        Assert.Same(userName, passwordConnectionInfo.Username);
        Assert.NotNull(passwordConnectionInfo.AuthenticationMethods);
        Assert.Single(passwordConnectionInfo.AuthenticationMethods);

        var passwordAuthentication = passwordConnectionInfo.AuthenticationMethods[0] as PasswordAuthenticationMethod;
        Assert.NotNull(passwordAuthentication);
        Assert.Equal(userName, passwordAuthentication.Username);
        Assert.True(Encoding.UTF8.GetBytes(password).IsEqualTo(passwordAuthentication.Password));
    }

    [Fact]
    public void Ctor_HostAndPortAndUsernameAndPrivateKeys()
    {
        var host = _random.Next().ToString();
        var port = _random.Next(1, 100);
        var userName = _random.Next().ToString();
        var privateKeys = new[] { GetRsaKey(), GetEcdsaKey() };

        var client = new ScpClient(host, port, userName, privateKeys);
        Assert.Equal(16 * 1024U, client.BufferSize);
        Assert.NotNull(client.ConnectionInfo);
        Assert.False(client.IsConnected);
        Assert.Equal(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
        Assert.Equal(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
        Assert.Same(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
        Assert.Null(client.Session);

        var privateKeyConnectionInfo = client.ConnectionInfo as PrivateKeyConnectionInfo;
        Assert.NotNull(privateKeyConnectionInfo);
        Assert.Equal(host, privateKeyConnectionInfo.Host);
        Assert.Equal(port, privateKeyConnectionInfo.Port);
        Assert.Same(userName, privateKeyConnectionInfo.Username);
        Assert.NotNull(privateKeyConnectionInfo.AuthenticationMethods);
        Assert.Single(privateKeyConnectionInfo.AuthenticationMethods);

        var privateKeyAuthentication = privateKeyConnectionInfo.AuthenticationMethods[0] as PrivateKeyAuthenticationMethod;
        Assert.NotNull(privateKeyAuthentication);
        Assert.Equal(userName, privateKeyAuthentication.Username);
        Assert.NotNull(privateKeyAuthentication.KeyFiles);
        Assert.Equal(privateKeys.Length, privateKeyAuthentication.KeyFiles.Count);
        Assert.True(privateKeyAuthentication.KeyFiles.Contains(privateKeys[0]));
        Assert.True(privateKeyAuthentication.KeyFiles.Contains(privateKeys[1]));
    }

    [Fact]
    public void Ctor_HostAndUsernameAndPrivateKeys()
    {
        var host = _random.Next().ToString();
        var userName = _random.Next().ToString();
        var privateKeys = new[] { GetRsaKey(), GetEcdsaKey() };

        var client = new ScpClient(host, userName, privateKeys);
        Assert.Equal(16 * 1024U, client.BufferSize);
        Assert.NotNull(client.ConnectionInfo);
        Assert.False(client.IsConnected);
        Assert.Equal(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
        Assert.Equal(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
        Assert.Same(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
        Assert.Null(client.Session);

        var privateKeyConnectionInfo = client.ConnectionInfo as PrivateKeyConnectionInfo;
        Assert.NotNull(privateKeyConnectionInfo);
        Assert.Equal(host, privateKeyConnectionInfo.Host);
        Assert.Equal(22, privateKeyConnectionInfo.Port);
        Assert.Same(userName, privateKeyConnectionInfo.Username);
        Assert.NotNull(privateKeyConnectionInfo.AuthenticationMethods);
        Assert.Single(privateKeyConnectionInfo.AuthenticationMethods);

        var privateKeyAuthentication = privateKeyConnectionInfo.AuthenticationMethods[0] as PrivateKeyAuthenticationMethod;
        Assert.NotNull(privateKeyAuthentication);
        Assert.Equal(userName, privateKeyAuthentication.Username);
        Assert.NotNull(privateKeyAuthentication.KeyFiles);
        Assert.Equal(privateKeys.Length, privateKeyAuthentication.KeyFiles.Count);
        Assert.True(privateKeyAuthentication.KeyFiles.Contains(privateKeys[0]));
        Assert.True(privateKeyAuthentication.KeyFiles.Contains(privateKeys[1]));
    }

    [Fact]
    public void RemotePathTransformation_Value_NotNull()
    {
        var client = new ScpClient("HOST", 22, "USER", "PWD");

        Assert.Same(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
        client.RemotePathTransformation = RemotePathTransformation.ShellQuote;
        Assert.Same(RemotePathTransformation.ShellQuote, client.RemotePathTransformation);
    }

    [Fact]
    public void RemotePathTransformation_Value_Null()
    {
        var client = new ScpClient("HOST", 22, "USER", "PWD")
        {
            RemotePathTransformation = RemotePathTransformation.ShellQuote
        };

        try
        {
            client.RemotePathTransformation = null;
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("value", ex.ParamName);
        }

        Assert.Same(RemotePathTransformation.ShellQuote, client.RemotePathTransformation);
    }

    private PrivateKeyFile GetRsaKey()
    {
        using (var stream = GetData("Key.RSA.txt"))
        {
            return new PrivateKeyFile(stream);
        }
    }

    private PrivateKeyFile GetEcdsaKey()
    {
        using (var stream = GetData("Key.ECDSA.txt"))
        {
            return new PrivateKeyFile(stream);
        }
    }
}
