using SilverAssertions;
using System;
using System.Collections.Generic;
using System.Threading;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Channels; //was previously: Renci.SshNet.Tests.Classes.Channels;

public class ChannelSessionTest_Dispose_SessionIsConnectedAndChannelIsOpen_ChannelCloseAndChannelEofReceived_SendChannelCloseMessageSuccess : ChannelSessionTestBase
{
    private uint _localChannelNumber;
    private uint _localWindowSize;
    private uint _localPacketSize;
    private uint _remoteChannelNumber;
    private uint _remoteWindowSize;
    private uint _remotePacketSize;
    private TimeSpan _channelCloseTimeout;
    private IList<ChannelEventArgs> _channelClosedRegister;
    private List<ExceptionEventArgs> _channelExceptionRegister;
    private ChannelSession _channel;
    private SemaphoreSlim _sessionSemaphore;
    private int _initialSessionSemaphoreCount;

    protected override void SetupData()
    {
        var random = new Random();

        _localChannelNumber = (uint)random.Next(0, int.MaxValue);
        _localWindowSize = (uint)random.Next(0, int.MaxValue);
        _localPacketSize = (uint)random.Next(0, int.MaxValue);
        _remoteChannelNumber = (uint)random.Next(0, int.MaxValue);
        _remoteWindowSize = (uint)random.Next(0, int.MaxValue);
        _remotePacketSize = (uint)random.Next(0, int.MaxValue);
        _channelCloseTimeout = TimeSpan.FromSeconds(random.Next(10, 20));
        _channelClosedRegister = new List<ChannelEventArgs>();
        _channelExceptionRegister = new List<ExceptionEventArgs>();
        _initialSessionSemaphoreCount = random.Next(10, 20);
        _sessionSemaphore = new SemaphoreSlim(_initialSessionSemaphoreCount);
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
                        p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)))
                    .Returns(true);
        SessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(ConnectionInfoMock.Object);
        ConnectionInfoMock.InSequence(sequence).Setup(p => p.ChannelCloseTimeout).Returns(_channelCloseTimeout);
        SessionMock.InSequence(sequence)
                   .Setup(p => p.TryWait(It.IsNotNull<WaitHandle>(), _channelCloseTimeout))
                   .Callback<WaitHandle, TimeSpan>((waitHandle, channelCloseTimeout) => waitHandle.WaitOne())
                   .Returns(WaitResult.Success);
    }

    protected override void Arrange()
    {
        base.Arrange();

        _channel = new ChannelSession(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
        _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
        _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
        _channel.Open();

        SessionMock.Raise(
            p => p.ChannelEofReceived += null,
            new MessageEventArgs<ChannelEofMessage>(new ChannelEofMessage(_localChannelNumber)));
        SessionMock.Raise(
            p => p.ChannelCloseReceived += null,
            new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
    }

    protected override void Act()
    {
        _channel.Dispose();
    }

    [Fact]
    public void CurrentCountOfSessionSemaphoreShouldBeEqualToInitialCount()
    {
        Assert.Equal(_initialSessionSemaphoreCount, _sessionSemaphore.CurrentCount);
    }

    [Fact]
    public void ExceptionShouldNeverHaveFired()
    {
        _channelExceptionRegister.Count.Should().Be(0, _channelExceptionRegister.AsString());
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
