using SilverAssertions;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connected_ServerSendsBadPacket : SessionTest_ConnectedBase
{
    private byte[] _packet;

    protected override void SetupData()
    {
        base.SetupData();

        _packet = new byte[] { 0x0a, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05 };
    }

    protected override void Act()
    {
        _ = ServerSocket.Send(_packet, 0, _packet.Length, SocketFlags.None);

        // give session some time to process packet
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
        Assert.Equal(DisconnectReason.ProtocolError, connectionException.DisconnectReason);
        Assert.Null(connectionException.InnerException);
        Assert.Equal("Bad packet length: 168101125.", connectionException.Message);
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
    public void ServerShouldBeDisconnected()
    {
        try
        {
            var buffer = new byte[1];

            var actual = ServerSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

            Assert.Equal(0, actual); // FIN
        }
        catch (SocketException sx)
        {
            Assert.Equal(SocketError.ConnectionReset, sx.SocketErrorCode); // RST
        }
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
            Assert.Equal(DisconnectReason.ProtocolError, ex.DisconnectReason);
            Assert.Null(ex.InnerException);
            Assert.Equal("Bad packet length: 168101125.", ex.Message);
        }
    }

    [Fact]
    public void ISession_WaitOnHandleAndTimeout_WaitHandle_ShouldThrowSshConnectionExceptionDetailingBadPacket()
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
            Assert.Equal(DisconnectReason.ProtocolError, ex.DisconnectReason);
            Assert.Null(ex.InnerException);
            Assert.Equal("Bad packet length: 168101125.", ex.Message);
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
