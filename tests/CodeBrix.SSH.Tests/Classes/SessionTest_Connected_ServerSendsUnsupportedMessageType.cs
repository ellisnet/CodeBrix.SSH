using SilverAssertions;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// This test verifies the current behavior, but this is not necessarily the behavior we want.
/// We should consider treating any exception as a "disconnect" since we're effectively interrupting
/// the message loop.
/// </summary>
public class SessionTest_Connected_ServerSendsUnsupportedMessageType : SessionTest_ConnectedBase
{
    private byte[] _packet;

    protected override void SetupData()
    {
        base.SetupData();

        _packet = CreatePacketForUnsupportedMessageType();
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
        Assert.Equal(typeof(SshException), exception.GetType());

        var sshException = (SshException)exception;
        Assert.Null(sshException.InnerException);
        Assert.Equal("Message type 255 is not supported.", sshException.Message);
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
    public void ReceiveOnServerSocketShouldTimeout()
    {
        var buffer = new byte[1];

        ServerSocket.ReceiveTimeout = 500;
        try
        {
            _ = ServerSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SocketException ex)
        {
            Assert.Equal(SocketError.TimedOut, ex.SocketErrorCode);
        }
    }

    [Fact]
    public void SendMessageShouldSendMessageToServer()
    {
        byte[] bytesReceivedByServer = null;
        ServerListener.BytesReceived += (received, socket) => bytesReceivedByServer = received;

        Session.SendMessage(new IgnoreMessage());

        // allow "server" some time to receive message
        Thread.Sleep(100);

        Assert.NotNull(bytesReceivedByServer);
        Assert.Equal(24, bytesReceivedByServer.Length);
    }

    [Fact]
    public void ISession_MessageListenerCompletedShouldBeSignaled()
    {
        var session = (ISession)Session;

        Assert.NotNull(session.MessageListenerCompleted);
        Assert.True(session.MessageListenerCompleted.WaitOne());
    }

    [Fact]
    public void ISession_SendMessageShouldSendMessageToServer()
    {
        var session = (ISession)Session;

        byte[] bytesReceivedByServer = null;
        ServerListener.BytesReceived += (received, socket) => bytesReceivedByServer = received;

        session.SendMessage(new IgnoreMessage());

        // allow "server" some time to receive message
        Thread.Sleep(100);

        Assert.NotNull(bytesReceivedByServer);
        Assert.Equal(24, bytesReceivedByServer.Length);
    }

    [Fact]
    public void ISession_TrySendMessageShouldReturnTrueAndSendMessageToServer()
    {
        var session = (ISession)Session;

        byte[] bytesReceivedByServer = null;
        ServerListener.BytesReceived += (received, socket) => bytesReceivedByServer = received;

        var actual = session.TrySendMessage(new IgnoreMessage());

        Assert.True(actual);

        // allow "server" some time to receive message
        Thread.Sleep(100);

        Assert.NotNull(bytesReceivedByServer);
        Assert.Equal(24, bytesReceivedByServer.Length);
    }

    [Fact]
    public void ISession_WaitOnHandleShouldThrowSshExceptionDetailingError()
    {
        var session = (ISession)Session;
        var waitHandle = new ManualResetEvent(false);

        try
        {
            session.WaitOnHandle(waitHandle);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("Message type 255 is not supported.", ex.Message);
        }
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeout_ShouldReturnFailed()
    {
        var session = (ISession)Session;
        var waitHandle = new ManualResetEvent(false);

        var result = session.TryWait(waitHandle, Timeout.InfiniteTimeSpan);

        Assert.Equal(WaitResult.Failed, result);
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldReturnFailed()
    {
        var session = (ISession)Session;
        var waitHandle = new ManualResetEvent(false);

        var result = session.TryWait(waitHandle, Timeout.InfiniteTimeSpan, out var exception);

        Assert.Equal(WaitResult.Failed, result);
        Assert.NotNull(exception);
        Assert.Equal(typeof(SshException), exception.GetType());

        var sshException = exception as SshException;
        Assert.NotNull(sshException);
        Assert.Null(sshException.InnerException);
        Assert.Equal("Message type 255 is not supported.", sshException.Message);
    }

    private static byte[] CreatePacketForUnsupportedMessageType()
    {
        byte messageType = 255;
        byte messageLength = 1;
        byte paddingLength = 10;
        var packetDataLength = (uint)messageLength + paddingLength + 1;

        var sshDataStream = new SshDataStream(4 + 1 + messageLength + paddingLength);
        sshDataStream.Write(packetDataLength);
        sshDataStream.WriteByte(paddingLength);
        sshDataStream.WriteByte(messageType);
        sshDataStream.Write(new byte[paddingLength]);

        return sshDataStream.ToArray();
    }
}
