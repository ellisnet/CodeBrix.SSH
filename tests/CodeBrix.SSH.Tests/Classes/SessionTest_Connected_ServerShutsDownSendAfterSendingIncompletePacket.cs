using SilverAssertions;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connected_ServerShutsDownSendAfterSendingIncompletePacket : SessionTest_ConnectedBase
{
    protected override void Act()
    {
        var incompletePacket = new byte[] { 0x0a, 0x05, 0x05 };

        _ = ServerSocket.Send(incompletePacket, 0, incompletePacket.Length, SocketFlags.None);

        // give session some time to start reading packet
        Thread.Sleep(100);

        ServerSocket.Shutdown(SocketShutdown.Send);

        // give session some time to process shut down of server socket
        Thread.Sleep(100);
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
    public void SendMessageShouldSucceed()
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
    public void ISession_SendMessageShouldSucceed()
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
    public void ISession_TrySendMessageShouldReturnTrue()
    {
        var session = (ISession)Session;

        Assert.False(session.TrySendMessage(new IgnoreMessage()));
    }

    [Fact]
    public void ISession_WaitOnHandle_WaitHandle_ShouldThrowSshConnectionExceptionDetailingBadPacket()
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
            Assert.Equal(DisconnectReason.ConnectionLost, ex.DisconnectReason);
            Assert.Null(ex.InnerException);
            Assert.Equal("An established connection was aborted by the server.", ex.Message);
        }
    }

    [Fact]
    public void ISession_WaitOnHandle_WaitHandleAndTimeout_ShouldThrowSshConnectionExceptionDetailingBadPacket()
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
            Assert.Equal(DisconnectReason.ConnectionLost, ex.DisconnectReason);
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
    public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldReturnDisconnected()
    {
        var session = (ISession)Session;
        var waitHandle = new ManualResetEvent(false);

        var result = session.TryWait(waitHandle, Timeout.InfiniteTimeSpan, out var exception);

        Assert.Equal(WaitResult.Disconnected, result);
        Assert.Null(exception);
    }
}
