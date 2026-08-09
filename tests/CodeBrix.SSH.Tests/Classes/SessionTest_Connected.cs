using SilverAssertions;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CodeBrix.SSH.Messages.Connection;
using CodeBrix.SSH.Messages.Transport;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connected : SessionTest_ConnectedBase
{
    private IgnoreMessage _ignoreMessage;

    protected override void SetupData()
    {
        base.SetupData();

        var data = new byte[10];
        Random.NextBytes(data);
        _ignoreMessage = new IgnoreMessage(data);
    }

    protected override void Act()
    {
    }

    [Fact]
    public void ClientVersionIsCodeBrixSsh()
    {
        Assert.Matches(
            // Ends with the four-segment CodeBrix date-stamped version, e.g. 1.0.221.978,
            // plus some optional metadata not containing '-'
            @"^SSH-2\.0-CodeBrix\.SSH\.SshClient\.\d+\.\d+\.\d+\.\d+(_[a-zA-Z0-9_\.]+)?$",
            Session.ClientVersion);
    }

    [Fact]
    public void IncludeStrictKexPseudoAlgorithmInInitKex()
    {
        Assert.True(FirstKexReceived.Wait(1000, TestContext.Current.CancellationToken));
        Assert.True(ServerBytesReceivedRegister.Count > 0);

        var kexInitMessage = new KeyExchangeInitMessage();
        kexInitMessage.Load(ServerBytesReceivedRegister[0], 4 + 1 + 1, ServerBytesReceivedRegister[0].Length - 4 - 1 - 1);
        Assert.True(kexInitMessage.KeyExchangeAlgorithms.Contains("kex-strict-c-v00@openssh.com"));
    }

    [Fact]
    public void ShouldNotIncludeStrictKexPseudoAlgorithmInSubsequentKex()
    {
        Assert.True(FirstKexReceived.Wait(1000, TestContext.Current.CancellationToken));

        using var subsequentKexReceived = new ManualResetEventSlim();
        bool kexContainsPseudoAlg = true;

        ServerListener.BytesReceived += ServerListener_BytesReceived;

        void ServerListener_BytesReceived(byte[] bytesReceived, Socket socket)
        {
            if (bytesReceived.Length > 5 && bytesReceived[5] == 20)
            {
                // SSH_MSG_KEXINIT = 20
                var kexInitMessage = new KeyExchangeInitMessage();
                kexInitMessage.Load(bytesReceived, 6, bytesReceived.Length - 6);
                kexContainsPseudoAlg = kexInitMessage.KeyExchangeAlgorithms.Contains("kex-strict-c-v00@openssh.com");
                subsequentKexReceived.Set();
            }
        }

        Session.SendMessage(Session.ClientInitMessage);

        Assert.True(subsequentKexReceived.Wait(1000, TestContext.Current.CancellationToken));
        Assert.False(kexContainsPseudoAlg);

        ServerListener.BytesReceived -= ServerListener_BytesReceived;
    }

    [Fact]
    public void ConnectionInfoShouldReturnConnectionInfoPassedThroughConstructor()
    {
        Assert.Same(ConnectionInfo, Session.ConnectionInfo);
    }

    [Fact]
    public void IsConnectedShouldReturnTrue()
    {
        Assert.True(Session.IsConnected);
    }

    [Fact]
    public void SendMessageShouldSendPacketToServer()
    {
        Thread.Sleep(100);

        ServerBytesReceivedRegister.Clear();

        Session.SendMessage(_ignoreMessage);

        // give session time to process message
        Thread.Sleep(100);

        Assert.Single(ServerBytesReceivedRegister);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UnknownGlobalRequestWithWantReply(bool wantReply)
    {
        Thread.Sleep(100);

        ServerBytesReceivedRegister.Clear();

        var globalRequest =
            new GlobalRequestMessage(Encoding.ASCII.GetBytes("unknown-request"), wantReply).GetPacket(8, null);

        ServerSocket.Send(globalRequest, 4, globalRequest.Length - 4, SocketFlags.None);

        Thread.Sleep(100);

        if (wantReply)
        {
            // Should have sent a failure reply.
            Assert.Single(ServerBytesReceivedRegister);
            ServerBytesReceivedRegister[0][5].Should().Be(82, "Expected to have sent SSH_MSG_REQUEST_FAILURE(82)");
        }
        else
        {
            // Should not have sent any reply.
            Assert.Empty(ServerBytesReceivedRegister);
        }

        Assert.Empty(ErrorOccurredRegister);
    }

    [Fact]
    public void SessionIdShouldReturnExchangeHashCalculatedFromKeyExchangeInitMessage()
    {
        Assert.NotNull(Session.SessionId);
        Assert.Same(SessionId, Session.SessionId);
    }

    [Fact]
    public void ServerVersionShouldNotReturnNull()
    {
        Assert.NotNull(Session.ServerVersion);
        Assert.Equal("SSH-2.0-OurServerStub", Session.ServerVersion);
    }

    [Fact]
    public void WaitOnHandle_WaitHandle_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
    {
        const WaitHandle waitHandle = null;

        try
        {
            Session.WaitOnHandle(waitHandle);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("waitHandle", ex.ParamName);
        }
    }

    [Fact]
    public void WaitOnHandle_WaitHandleAndTimeout_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
    {
        const WaitHandle waitHandle = null;
        var timeout = TimeSpan.FromMinutes(5);

        try
        {
            Session.WaitOnHandle(waitHandle, timeout);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("waitHandle", ex.ParamName);
        }
    }

    [Fact]
    public void ISession_ConnectionInfoShouldReturnConnectionInfoPassedThroughConstructor()
    {
        var session = (ISession)Session;
        Assert.Same(ConnectionInfo, session.ConnectionInfo);
    }

    [Fact]
    public void ISession_MessageListenerCompletedShouldNotBeSignaled()
    {
        var session = (ISession)Session;

        Assert.NotNull(session.MessageListenerCompleted);
        Assert.False(session.MessageListenerCompleted.WaitOne(0));
    }

    [Fact]
    public void ISession_SendMessageShouldSendPacketToServer()
    {
        Thread.Sleep(100);

        var session = (ISession)Session;
        ServerBytesReceivedRegister.Clear();

        session.SendMessage(_ignoreMessage);

        // give session time to process message
        Thread.Sleep(100);

        Assert.Single(ServerBytesReceivedRegister);
    }

    [Fact]
    public void ISession_TrySendMessageShouldSendPacketToServerAndReturnTrue()
    {
        Thread.Sleep(100);

        var session = (ISession)Session;
        ServerBytesReceivedRegister.Clear();

        var actual = session.TrySendMessage(new IgnoreMessage());

        // give session time to process message
        Thread.Sleep(100);

        Assert.True(actual);
        Assert.Single(ServerBytesReceivedRegister);
    }

    [Fact]
    public void ISession_WaitOnHandleShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
    {
        const WaitHandle waitHandle = null;
        var session = (ISession)Session;

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
    public void ISession_TryWait_WaitHandleAndTimeout_ShouldReturnSuccessIfWaitHandleIsSignaled()
    {
        var session = (ISession)Session;
        var waitHandle = new ManualResetEvent(true);

        var result = session.TryWait(waitHandle, TimeSpan.FromMilliseconds(0));

        Assert.Equal(WaitResult.Success, result);
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeout_ShouldReturnTimedOutIfWaitHandleIsNotSignaled()
    {
        var session = (ISession)Session;
        var waitHandle = new ManualResetEvent(false);

        var result = session.TryWait(waitHandle, TimeSpan.FromMilliseconds(0));

        Assert.Equal(WaitResult.TimedOut, result);
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeout_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
    {
        var session = (ISession)Session;
        const WaitHandle waitHandle = null;

        try
        {
            _ = session.TryWait(waitHandle, Timeout.InfiniteTimeSpan);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("waitHandle", ex.ParamName);
        }
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldReturnSuccessIfWaitHandleIsSignaled()
    {
        var session = (ISession)Session;
        var waitHandle = new ManualResetEvent(true);

        var result = session.TryWait(waitHandle, TimeSpan.FromMilliseconds(0), out var exception);

        Assert.Equal(WaitResult.Success, result);
        Assert.Null(exception);
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldReturnTimedOutIfWaitHandleIsNotSignaled()
    {
        var session = (ISession)Session;
        var waitHandle = new ManualResetEvent(false);

        var result = session.TryWait(waitHandle, TimeSpan.FromMilliseconds(0), out var exception);

        Assert.Equal(WaitResult.TimedOut, result);
        Assert.Null(exception);
    }

    [Fact]
    public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
    {
        var session = (ISession)Session;
        const WaitHandle waitHandle = null;
        Exception exception = null;

        try
        {
            session.TryWait(waitHandle, Timeout.InfiniteTimeSpan, out exception);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("waitHandle", ex.ParamName);
            Assert.Null(exception);
        }
    }

    [Fact]
    public void ClientSocketShouldBeConnected()
    {
        Assert.NotNull(ClientSocket);
        Assert.True(ClientSocket.Connected);
    }

    [Fact]
    public void CreateConnectorOnServiceFactoryShouldHaveBeenInvokedOnce()
    {
        ServiceFactoryMock.Verify(p => p.CreateConnector(ConnectionInfo, SocketFactoryMock.Object), Times.Once());
    }

    [Fact]
    public void ConnectorOnConnectorShouldHaveBeenInvokedOnce()
    {
        ConnectorMock.Verify(p => p.Connect(ConnectionInfo), Times.Once());
    }
}
