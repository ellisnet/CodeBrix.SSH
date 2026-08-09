using System;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Connection;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class BaseClientTest_ConnectAsync_Timeout : IDisposable
{
    private BaseClient _client;

    public BaseClientTest_ConnectAsync_Timeout()
    {
        Init();
    }

    private void Init()
    {
        var sessionMock = new Mock<ISession>();
        sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        var serviceFactoryMock = new Mock<IServiceFactory>();
        var socketFactoryMock = new Mock<ISocketFactory>();

        sessionMock.Setup(p => p.ConnectAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(c => Task.Delay(Timeout.Infinite, c));

        serviceFactoryMock.Setup(p => p.CreateSocketFactory())
                                           .Returns(socketFactoryMock.Object);

        var connectionInfo = new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "pwd"))
        {
            Timeout = TimeSpan.FromSeconds(1)
        };

        serviceFactoryMock.Setup(p => p.CreateSession(connectionInfo, socketFactoryMock.Object))
                               .Returns(sessionMock.Object);

        _client = new MyClient(connectionInfo, false, serviceFactoryMock.Object);
    }

    [Fact]
    public async Task ConnectAsyncWithTimeoutThrowsSshTimeoutException()
    {
        await Assert.ThrowsAsync<SshOperationTimeoutException>(() => _client.ConnectAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ConnectAsyncWithCancelledTokenThrowsOperationCancelledException()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(() => _client.ConnectAsync(cancellationTokenSource.Token));
    }

    public void Dispose()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        _client?.Dispose();
    }

    private class MyClient : BaseClient
    {
        public MyClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory) : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
        }
    }
}
