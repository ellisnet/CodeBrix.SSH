using System;
using System.Net;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_ConnectToServerFails : SessionTestBase
{
    private ConnectionInfo _connectionInfo;
    private Session _session;
    private SshConnectionException _connectException;
    private SshConnectionException _actualException;

    protected override void SetupData()
    {
        base.SetupData();

        var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
        _connectionInfo = CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5));
        _session = new Session(_connectionInfo, ServiceFactoryMock.Object, SocketFactoryMock.Object);
        _connectException = new SshConnectionException();
    }

    protected override void SetupMocks()
    {
        base.SetupMocks();

        _ = ServiceFactoryMock.Setup(p => p.CreateConnector(_connectionInfo, SocketFactoryMock.Object))
                               .Returns(ConnectorMock.Object);
        _ = ConnectorMock.Setup(p => p.Connect(_connectionInfo))
                          .Throws(_connectException);
    }

    protected override void Act()
    {
        try
        {
            _session.Connect();
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshConnectionException ex)
        {
            _actualException = ex;
        }
    }

    [Fact]
    public void ConnectionInfoShouldReturnConnectionInfoPassedThroughConstructor()
    {
        Assert.Same(_connectionInfo, _session.ConnectionInfo);
    }

    [Fact]
    public void ConnectShouldHaveRethrownException()
    {
        Assert.NotNull(_actualException);
        Assert.Same(_connectException, _actualException);
    }

    [Fact]
    public void DisconnectShouldNotThrowAnException()
    {
        _session.Disconnect();
    }

    [Fact]
    public void DisposeShouldNotThrowException()
    {
        _session.Dispose();
    }

    [Fact]
    public void IsConnectedShouldReturnFalse()
    {
        Assert.False(_session.IsConnected);
    }

    [Fact]
    public void SendMessageShouldThrowSshConnectionException()
    {
        try
        {
            _session.SendMessage(new IgnoreMessage());
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshConnectionException ex)
        {
            Assert.Equal(DisconnectReason.None, ex.DisconnectReason);
            Assert.Null(ex.InnerException);
            Assert.Equal("Client not connected.", ex.Message);
        }
    }

    [Fact]
    public void SessionIdShouldReturnNull()
    {
        Assert.Null(_session.SessionId);
    }

    [Fact]
    public void ServerVersionShouldReturnNull()
    {
        Assert.Null(_session.ServerVersion);
    }

    [Fact]
    public void WaitOnHandle_WaitOnHandle_WaitHandle_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
    {
        const WaitHandle waitHandle = null;

        try
        {
            _session.WaitOnHandle(waitHandle);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("waitHandle", ex.ParamName);
        }
    }

    [Fact]
    public void WaitOnHandle_WaitOnHandle_WaitHandleAndTimeout_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
    {
        const WaitHandle waitHandle = null;
        var timeout = TimeSpan.FromMinutes(5);

        try
        {
            _session.WaitOnHandle(waitHandle, timeout);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("waitHandle", ex.ParamName);
        }
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeout_ShouldReturnDisconnected()
    {
        var session = (ISession)_session;
        var waitHandle = new ManualResetEvent(false);

        var result = session.TryWait(waitHandle, Timeout.InfiniteTimeSpan);

        Assert.Equal(WaitResult.Disconnected, result);
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldReturnDisconnected()
    {
        var session = (ISession)_session;
        var waitHandle = new ManualResetEvent(false);

        var result = session.TryWait(waitHandle, Timeout.InfiniteTimeSpan, out var exception);

        Assert.Equal(WaitResult.Disconnected, result);
        Assert.Null(exception);
    }

    [Fact]
    public void ISession_ConnectionInfoShouldReturnConnectionInfoPassedThroughConstructor()
    {
        var session = (ISession)_session;
        Assert.Same(_connectionInfo, session.ConnectionInfo);
    }

    [Fact]
    public void ISession_MessageListenerCompletedShouldBeSignaled()
    {
        var session = (ISession)_session;

        Assert.NotNull(session.MessageListenerCompleted);
        Assert.True(session.MessageListenerCompleted.WaitOne(0));
    }

    [Fact]
    public void ISession_SendMessageShouldThrowSshConnectionException()
    {
        var session = (ISession)_session;

        try
        {
            session.SendMessage(new IgnoreMessage());
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshConnectionException ex)
        {
            Assert.Equal(DisconnectReason.None, ex.DisconnectReason);
            Assert.Null(ex.InnerException);
            Assert.Equal("Client not connected.", ex.Message);
        }
    }

    [Fact]
    public void ISession_TrySendMessageShouldReturnFalse()
    {
        var session = (ISession)_session;

        var actual = session.TrySendMessage(new IgnoreMessage());

        Assert.False(actual);
    }

    [Fact]
    public void ISession_WaitOnHandle_WaitHandle_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
    {
        const WaitHandle waitHandle = null;
        var session = (ISession)_session;

        try
        {
            session.WaitOnHandle(waitHandle);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("waitHandle", ex.ParamName);
        }
    }

    [Fact]
    public void ISession_WaitOnHandle_WaitHandleAndTimeout_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
    {
        const WaitHandle waitHandle = null;
        var session = (ISession)_session;

        try
        {
            session.WaitOnHandle(waitHandle, Timeout.InfiniteTimeSpan);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("waitHandle", ex.ParamName);
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
