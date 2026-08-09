using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Channels; //was previously: Renci.SshNet.Tests.Classes.Channels;

public class ChannelSessionTest_Open_OnOpenFailureReceived_NoRetriesAvailable : ChannelSessionTestBase
{
    private ChannelSession _channel;
    private uint _localChannelNumber;
    private uint _localWindowSize;
    private uint _localPacketSize;
    private IList<ChannelEventArgs> _channelClosedRegister;
    private List<ExceptionEventArgs> _channelExceptionRegister;
    private SemaphoreSlim _sessionSemaphore;
    private int _initialSessionSemaphoreCount;
    private uint _failureReasonCode;
    private string _failureDescription;
    private string _failureLanguage;
    private SshException _actualException;

    protected override void SetupData()
    {
        var random = new Random();

        _localChannelNumber = (uint)random.Next(0, int.MaxValue);
        _localWindowSize = (uint)random.Next(2000, 3000);
        _localPacketSize = (uint)random.Next(1000, 2000);
        _initialSessionSemaphoreCount = random.Next(10, 20);
        _sessionSemaphore = new SemaphoreSlim(_initialSessionSemaphoreCount);
        _channelClosedRegister = new List<ChannelEventArgs>();
        _channelExceptionRegister = new List<ExceptionEventArgs>();
        _actualException = null;

        _failureReasonCode = (uint)random.Next(0, int.MaxValue);
        _failureDescription = random.Next().ToString(CultureInfo.InvariantCulture);
        _failureLanguage = random.Next().ToString(CultureInfo.InvariantCulture);
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
                            s => s.ChannelOpenFailureReceived += null,
                            new MessageEventArgs<ChannelOpenFailureMessage>(
                                new ChannelOpenFailureMessage(
                                    _localChannelNumber,
                                    _failureDescription,
                                    _failureReasonCode,
                                    _failureLanguage
                                    )));
                        w.WaitOne();
                    });
        SessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(ConnectionInfoMock.Object);
        ConnectionInfoMock.InSequence(sequence).Setup(p => p.RetryAttempts).Returns(1);
    }

    protected override void Arrange()
    {
        base.Arrange();

        _channel = new ChannelSession(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
        _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
        _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
    }

    protected override void Act()
    {
        try
        {
            _channel.Open();
        }
        catch (SshException ex)
        {
            _actualException = ex;
        }
    }

    [Fact]
    public void OpenShouldHaveThrownSshException()
    {
        Assert.NotNull(_actualException);
        Assert.Equal(typeof(SshException), _actualException.GetType());
        Assert.Null(_actualException.InnerException);
        Assert.Equal("Failed to open a channel after 1 attempts.", _actualException.Message);
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
    public void ClosedEventShouldNeverHaveFired()
    {
        Assert.Empty(_channelClosedRegister);
    }

    [Fact]
    public void IsOpenShouldReturnFalse()
    {
        Assert.False(_channel.IsOpen);
    }
}
