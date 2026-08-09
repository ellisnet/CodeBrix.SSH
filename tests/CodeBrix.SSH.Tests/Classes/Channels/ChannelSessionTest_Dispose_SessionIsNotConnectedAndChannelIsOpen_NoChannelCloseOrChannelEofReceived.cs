using System;
using System.Collections.Generic;
using System.Threading;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Channels; //was previously: Renci.SshNet.Tests.Classes.Channels;

public class ChannelSessionTest_Dispose_SessionIsNotConnectedAndChannelIsOpen_NoChannelCloseOrChannelEofReceived : ChannelSessionTestBase
{
    private uint _localChannelNumber;
    private uint _localWindowSize;
    private uint _localPacketSize;
    private uint _remoteChannelNumber;
    private uint _remoteWindowSize;
    private uint _remotePacketSize;
    private IList<ChannelEventArgs> _channelClosedRegister;
    private List<ExceptionEventArgs> _channelExceptionRegister;
    private ChannelSession _channel;
    private MockSequence _sequence;
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
        _channelClosedRegister = new List<ChannelEventArgs>();
        _channelExceptionRegister = new List<ExceptionEventArgs>();
        _initialSessionSemaphoreCount = random.Next(10, 20);
        _sessionSemaphore = new SemaphoreSlim(_initialSessionSemaphoreCount);
    }

    protected override void SetupMocks()
    {
        _sequence = new MockSequence();
        SessionMock.InSequence(_sequence).Setup(p => p.ConnectionInfo).Returns(ConnectionInfoMock.Object);
        ConnectionInfoMock.InSequence(_sequence).Setup(p => p.RetryAttempts).Returns(1);
        SessionMock.Setup(p => p.SessionSemaphore).Returns(_sessionSemaphore);
        SessionMock.InSequence(_sequence)
                    .Setup(
                        p =>
                            p.SendMessage(
                                It.Is<ChannelOpenMessage>(
                                    m =>
                                        m.LocalChannelNumber == _localChannelNumber &&
                                        m.InitialWindowSize == _localWindowSize && m.MaximumPacketSize == _localPacketSize &&
                                        m.Info is SessionChannelOpenInfo)));
        SessionMock.InSequence(_sequence)
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
        SessionMock.Setup(p => p.IsConnected).Returns(false);
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
    public void CurrentCountOfSessionSemaphoreShouldBeEqualToInitialCount()
    {
        Assert.Equal(_initialSessionSemaphoreCount, _sessionSemaphore.CurrentCount);
    }

    [Fact]
    public void ExceptionShouldNeverHaveFired()
    {
        Assert.Empty(_channelExceptionRegister);
    }

    [Fact]
    public void ClosedEventShouldNotHaveFired()
    {
        Assert.Empty(_channelClosedRegister);
    }

    [Fact]
    public void IsOpenShouldReturnFalse()
    {
        Assert.False(_channel.IsOpen);
    }
}
