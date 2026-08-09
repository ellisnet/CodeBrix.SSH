using SilverAssertions;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connected_Disconnect : SessionTest_ConnectedBase
{
    protected override void Act()
    {
        using ManualResetEventSlim clientDisconnected = new ManualResetEventSlim(false);
        ServerListener.Disconnected += (socket) => clientDisconnected.Set();
        Session.Disconnect();
        clientDisconnected.Wait(10000);
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
    public void ErrorOccurredIsNeverRaised()
    {
        ErrorOccurredRegister.Count.Should().Be(0, ErrorOccurredRegister.AsString());
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
            Assert.Equal(DisconnectReason.None, ex.DisconnectReason);
            Assert.Null(ex.InnerException);
            Assert.Equal("Client not connected.", ex.Message);
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
            Assert.Equal(DisconnectReason.None, ex.DisconnectReason);
            Assert.Null(ex.InnerException);
            Assert.Equal("Client not connected.", ex.Message);
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

    [Fact]
    public void ClientSocketShouldNotBeConnected()
    {
        Assert.NotNull(ClientSocket);
        Assert.False(ClientSocket.Connected);
    }
}
