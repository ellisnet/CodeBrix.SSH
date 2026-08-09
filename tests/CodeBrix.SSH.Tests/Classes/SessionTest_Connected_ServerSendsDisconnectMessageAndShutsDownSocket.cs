using SilverAssertions;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connected_ServerSendsDisconnectMessageAndShutsDownSocket : SessionTest_ConnectedBase
{
    private DisconnectMessage _disconnectMessage;

    protected override void SetupData()
    {
        base.SetupData();

        _disconnectMessage = new DisconnectMessage(DisconnectReason.ServiceNotAvailable, "Not today!");
    }

    protected override void Act()
    {
        // server sends SSH_MSG_DISCONNECT
        var disconnect = _disconnectMessage.GetPacket(8, null);

        _ = ServerSocket.Send(disconnect, 4, disconnect.Length - 4, SocketFlags.None);

        // server shuts down the socket
        ServerSocket.Shutdown(SocketShutdown.Send);

        // give session some time to process DisconnectMessage and socket shutdown
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
    public void DisconnectedIsRaisedOnce()
    {
        Assert.Single(DisconnectedRegister);
    }

    [Fact]
    public void DisconnectReceivedIsRaisedOnce()
    {
        Assert.Single(DisconnectReceivedRegister);

        var disconnectMessage = DisconnectReceivedRegister[0].Message;
        Assert.NotNull(disconnectMessage);
        Assert.Equal(_disconnectMessage.Description, disconnectMessage.Description);
        Assert.Equal("en", disconnectMessage.Language);
        Assert.Equal(_disconnectMessage.ReasonCode, disconnectMessage.ReasonCode);
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
    public void ISession_WaitOnHandle_WaitHandle_ShouldThrowSshConnectionExceptionDetailingDisconnectReason()
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
            Assert.Equal(DisconnectReason.ServiceNotAvailable, ex.DisconnectReason);
            Assert.Null(ex.InnerException);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture,
                                          "The connection was closed by the server: {0} ({1}).",
                                          _disconnectMessage.Description,
                                          _disconnectMessage.ReasonCode),
                            ex.Message);
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
