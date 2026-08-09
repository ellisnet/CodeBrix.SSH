using SilverAssertions;
using System.Diagnostics;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connected_ConnectionReset : SessionTest_ConnectedBase
{
    protected override void Act()
    {
        ServerSocket.Close();

        // give session some time to react to connection reset
        Thread.Sleep(300);
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
    public void SendMessageShouldThrowSshConnectionException()
    {
        try
        {
            Session.SendMessage(new IgnoreMessage());
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
            Assert.Equal(DisconnectReason.None, ex.DisconnectReason);
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
    public void ISession_WaitOnHandle_WaitHandle_ShouldThrowSshConnectionException()
    {
        var session = (ISession)Session;
        using var waitHandle = new ManualResetEvent(false);

        var ex = Assert.Throws<SshConnectionException>(() => session.WaitOnHandle(waitHandle));

        Assert.Equal(DisconnectReason.ConnectionLost, ex.DisconnectReason);
    }

    [Fact]
    public void ISession_WaitOnHandle_WaitHandleAndTimeout_ShouldThrowSshConnectionException()
    {
        var session = (ISession)Session;
        using var waitHandle = new ManualResetEvent(false);

        var ex = Assert.Throws<SshConnectionException>(() => session.WaitOnHandle(waitHandle));

        Assert.Equal(DisconnectReason.ConnectionLost, ex.DisconnectReason);
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeout_ShouldReturnDisconnected()
    {
        var session = (ISession)Session;
        using var waitHandle = new ManualResetEvent(false);

        var result = session.TryWait(waitHandle, Timeout.InfiniteTimeSpan);

        Assert.Equal(WaitResult.Disconnected, result);
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldReturnDisconnected()
    {
        var session = (ISession)Session;
        using var waitHandle = new ManualResetEvent(false);

        var result = session.TryWait(waitHandle, Timeout.InfiniteTimeSpan, out var exception);

        Assert.Equal(WaitResult.Disconnected, result);
        Assert.Null(exception);
    }
}
