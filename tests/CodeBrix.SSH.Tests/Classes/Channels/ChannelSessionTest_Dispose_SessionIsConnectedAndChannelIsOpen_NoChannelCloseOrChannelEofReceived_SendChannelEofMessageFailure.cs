using System;
using System.Collections.Generic;
using System.Threading;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Channels; //was previously: Renci.SshNet.Tests.Classes.Channels;

public class ChannelSessionTest_Dispose_SessionIsConnectedAndChannelIsOpen_NoChannelCloseOrChannelEofReceived_SendChannelEofMessageFailure : ChannelSessionTestBase
{
    private ChannelSession _channel;
    private uint _localChannelNumber;
    private uint _localWindowSize;
    private uint _localPacketSize;
    private uint _remoteWindowSize;
    private uint _remotePacketSize;
    private uint _remoteChannelNumber;
    private TimeSpan _channelCloseTimeout;
    private SemaphoreSlim _sessionSemaphore;
    private IList<ChannelEventArgs> _channelClosedRegister;
    private List<ExceptionEventArgs> _channelExceptionRegister;

    protected override void SetupData()
    {
        var random = new Random();

        _localChannelNumber = (uint)random.Next(0, int.MaxValue);
        _localWindowSize = (uint)random.Next(2000, 3000);
        _localPacketSize = (uint)random.Next(1000, 2000);
        _remoteChannelNumber = (uint)random.Next(0, int.MaxValue);
        _remoteWindowSize = (uint)random.Next(0, int.MaxValue);
        _remotePacketSize = (uint)random.Next(100, 200);
        _channelCloseTimeout = TimeSpan.FromSeconds(random.Next(10, 20));
        _sessionSemaphore = new SemaphoreSlim(1);
        _channelClosedRegister = new List<ChannelEventArgs>();
        _channelExceptionRegister = new List<ExceptionEventArgs>();
    }

    protected override void SetupMocks()
    {
        var sequence = new MockSequence();

        SessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(ConnectionInfoMock.Object);
        ConnectionInfoMock.InSequence(sequence).Setup(p => p.RetryAttempts).Returns(1);
        SessionMock.Setup(p => p.SessionSemaphore).Returns(_sessionSemaphore);
        SessionMock.InSequence(sequence)
                    .Setup(
                        p =>
                            p.SendMessage(
                                It.Is<ChannelOpenMessage>(
                                    m =>
                                        m.LocalChannelNumber == _localChannelNumber &&
                                        m.InitialWindowSize == _localWindowSize && m.MaximumPacketSize == _localPacketSize &&
                                        m.Info is SessionChannelOpenInfo)));
        SessionMock.InSequence(sequence)
                    .Setup(p => p.WaitOnHandle(It.IsNotNull<WaitHandle>()))
                    .Callback<WaitHandle>(
                        w =>
                        {
                            SessionMock.Raise(
                                s => s.ChannelOpenConfirmationReceived += null,
                                new MessageEventArgs<ChannelOpenConfirmationMessage>(
                                    new ChannelOpenConfirmationMessage(
                                        _localChannelNumber,
                                        _remoteWindowSize,
                                        _remotePacketSize,
                                        _remoteChannelNumber)));
                            w.WaitOne();
                        });
        SessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
        SessionMock.InSequence(sequence)
                    .Setup(
                        p => p.TrySendMessage(It.Is<ChannelEofMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)))
                    .Returns(false);
        SessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
        SessionMock.InSequence(sequence)
                    .Setup(
                        p => p.TrySendMessage(It.Is<ChannelCloseMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)))
                    .Returns(true);
        SessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(ConnectionInfoMock.Object);
        ConnectionInfoMock.InSequence(sequence).Setup(p => p.ChannelCloseTimeout).Returns(_channelCloseTimeout);
        SessionMock.InSequence(sequence)
                   .Setup(p => p.TryWait(It.IsNotNull<WaitHandle>(), _channelCloseTimeout))
                   .Callback<WaitHandle, TimeSpan>(
                       (waitHandle, channelCloseTimeout) =>
                       {
                           SessionMock.Raise(s => s.ChannelCloseReceived += null,
                                             new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
                           waitHandle.WaitOne();
                       })
                   .Returns(WaitResult.Success);
    }

    protected override void Arrange()
    {
        base.Arrange();

        _channel = new ChannelSession(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
        _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
        _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
        _channel.Open();
    }

    protected override void Act()
    {
        _channel.Dispose();
    }

    [Fact]
    public void ChannelEofMessageShouldBeSentOnce()
    {
        SessionMock.Verify(p => p.TrySendMessage(It.Is<ChannelEofMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)), Times.Once);
    }

    [Fact]
    public void ChannelCloseMessageShouldBeSentOnce()
    {
        SessionMock.Verify(p => p.TrySendMessage(It.Is<ChannelCloseMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)), Times.Once);
    }

    [Fact]
    public void ExceptionShouldNeverHaveFired()
    {
        Assert.Empty(_channelExceptionRegister);
    }

    [Fact]
    public void ClosedEventShouldHaveFiredOnce()
    {
        Assert.Single(_channelClosedRegister);
        Assert.Equal(_localChannelNumber, _channelClosedRegister[0].ChannelNumber);
    }

    [Fact]
    public void IsOpenShouldReturnFalse()
    {
        Assert.False(_channel.IsOpen);
    }
}
