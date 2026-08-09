using SilverAssertions;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connected_ServerShutsDownSocket : SessionTest_ConnectedBase
{
    protected override void Act()
    {
        ServerSocket.Shutdown(SocketShutdown.Send);

        // give session some time to process socket shutdown
        Thread.Sleep(200);
    }

    [Fact]
    public void IsConnectedShouldReturnFalse()
    {
        Assert.False(Session.IsConnected);
    }

    [Fact]
    public void DisconnectShouldFinishImmediately()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        Session.Disconnect();

        stopwatch.Stop();
        Assert.True(stopwatch.ElapsedMilliseconds < 500);
    }

    [Fact]
    public void DisconnectedIsNeverRaised()
    {
        Assert.Empty(DisconnectedRegister);
    }

    [Fact]
    public void DisconnectReceivedIsNeverRaised()
    {
        Assert.Empty(DisconnectReceivedRegister);
    }

    [Fact]
    public void ErrorOccurredIsRaisedOnce()
    {
        ErrorOccurredRegister.Count.Should().Be(1, ErrorOccurredRegister.AsString());

        var errorOccurred = ErrorOccurredRegister[0];
        Assert.NotNull(errorOccurred);

        var exception = errorOccurred.Exception;
        Assert.NotNull(exception);
        Assert.Equal(typeof(SshConnectionException), exception.GetType());

        var connectionException = (SshConnectionException)exception;
        Assert.Equal(DisconnectReason.ConnectionLost, connectionException.DisconnectReason);
        Assert.Null(connectionException.InnerException);
        Assert.Equal("An established connection was aborted by the server.", connectionException.Message);
    }

    [Fact]
    public void DisposeShouldFinishImmediately()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        Session.Dispose();

        stopwatch.Stop();
        Assert.True(stopwatch.ElapsedMilliseconds < 500);
    }

    [Fact]
    public void ReceiveOnServerSocketShouldReturnZero()
    {
        var buffer = new byte[1];

        var actual = ServerSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

        Assert.Equal(0, actual);
    }

    [Fact]
    public void SendMessageShouldThrowSshConnectionException()
    {
        try
        {
            Session.SendMessage(new IgnoreMessage());
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshConnectionException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("Client not connected.", ex.Message);
        }
    }

    [Fact]
    public void ISession_MessageListenerCompletedShouldBeSignaled()
    {
        var session = (ISession)Session;

        Assert.NotNull(session.MessageListenerCompleted);
        Assert.True(session.MessageListenerCompleted.WaitOne());
    }

    [Fact]
    public void ISession_SendMessageShouldThrowSshConnectionException()
    {
        var session = (ISession)Session;

        try
        {
            session.SendMessage(new IgnoreMessage());
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshConnectionException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("Client not connected.", ex.Message);
        }
    }

    [Fact]
    public void ISession_TrySendMessageShouldReturnFalse()
    {
        var session = (ISession)Session;

        var actual = session.TrySendMessage(new IgnoreMessage());

        Assert.False(actual);
    }

    [Fact]
    public void ISession_WaitOnHandle_WaitHandle_ShouldThrowSshConnectionExceptionDetailingAbortedConnection()
    {
        var session = (ISession)Session;
        var waitHandle = new ManualResetEvent(false);

        try
        {
            session.WaitOnHandle(waitHandle);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshConnectionException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("An established connection was aborted by the server.", ex.Message);
        }
    }

    [Fact]
    public void ISession_WaitOnHandle_WaitHandleAndTimeout_ShouldThrowSshConnectionExceptionDetailingAbortedConnection()
    {
        var session = (ISession)Session;
        var waitHandle = new ManualResetEvent(false);

        try
        {
            session.WaitOnHandle(waitHandle, Timeout.InfiniteTimeSpan);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshConnectionException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("An established connection was aborted by the server.", ex.Message);
        }
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeout_ShouldReturnDisconnected()
    {
        var session = (ISession)Session;
        var waitHandle = new ManualResetEvent(false);

        var result = session.TryWait(waitHandle, Timeout.InfiniteTimeSpan);

        Assert.Equal(WaitResult.Disconnected, result);
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeout_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
    {
        var session = (ISession)Session;
        const WaitHandle waitHandle = null;
        var timeout = TimeSpan.FromMinutes(5);

        try
        {
            _ = session.TryWait(waitHandle, timeout);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("waitHandle", ex.ParamName);
        }
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldReturnDisconnected()
    {
        var session = (ISession)Session;
        var waitHandle = new ManualResetEvent(false);

        var result = session.TryWait(waitHandle, Timeout.InfiniteTimeSpan, out var exception);

        Assert.Equal(WaitResult.Disconnected, result);
        Assert.Null(exception);
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
    {
        var session = (ISession)Session;
        const WaitHandle waitHandle = null;
        var timeout = TimeSpan.FromMinutes(5);
        Exception exception = null;

        try
        {
            _ = session.TryWait(waitHandle, timeout, out exception);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("waitHandle", ex.ParamName);
        }

        Assert.Null(exception);
    }
}
