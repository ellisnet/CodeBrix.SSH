using SilverAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SubsystemSession_OnSessionErrorOccurred_Connected
{
    private Mock<ISession> _sessionMock;
    private Mock<IChannelSession> _channelMock;
    private string _subsystemName;
    private SubsystemSessionStub _subsystemSession;
    private int _operationTimeout;
    private IList<EventArgs> _disconnectedRegister;
    private IList<ExceptionEventArgs> _errorOccurredRegister;
    private ExceptionEventArgs _errorOccurredEventArgs;
    private MockSequence _sequence;

    public SubsystemSession_OnSessionErrorOccurred_Connected()
    {
        Setup();
    }

    private void Setup()
    {
        Arrange();
        Act();
    }

    protected void Arrange()
    {
        var random = new Random();
        _subsystemName = random.Next().ToString(CultureInfo.InvariantCulture);
        _operationTimeout = 30000;
        _disconnectedRegister = new List<EventArgs>();
        _errorOccurredRegister = new List<ExceptionEventArgs>();
        _errorOccurredEventArgs = new ExceptionEventArgs(new SystemException());

        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        _channelMock = new Mock<IChannelSession>(MockBehavior.Strict);

        _sequence = new MockSequence();
        _sessionMock.InSequence(_sequence).Setup(p => p.CreateChannelSession()).Returns(_channelMock.Object);
        _channelMock.InSequence(_sequence).Setup(p => p.Open());
        _channelMock.InSequence(_sequence).Setup(p => p.SendSubsystemRequest(_subsystemName)).Returns(true);

        _subsystemSession = new SubsystemSessionStub(
            _sessionMock.Object,
            _subsystemName,
            _operationTimeout);
        _subsystemSession.Disconnected += (sender, args) => _disconnectedRegister.Add(args);
        _subsystemSession.ErrorOccurred += (sender, args) => _errorOccurredRegister.Add(args);
        _subsystemSession.Connect();
    }

    protected void Act()
    {
        _sessionMock.Raise(s => s.ErrorOccured += null, _errorOccurredEventArgs);
    }

    [Fact]
    public void DisconnectHasNeverFired()
    {
        Assert.Empty(_disconnectedRegister);
    }

    [Fact]
    public void ErrorOccurredHasFiredOnce()
    {
        _errorOccurredRegister.Count.Should().Be(1, _errorOccurredRegister.AsString());
        _errorOccurredRegister[0].Exception.Should().BeSameAs(_errorOccurredEventArgs.Exception, _errorOccurredRegister.AsString());
    }

    [Fact]
    public void IsOpenShouldReturnTrueWhenChannelIsOpen()
    {
        _channelMock.InSequence(_sequence).Setup(p => p.IsOpen).Returns(true);

        Assert.True(_subsystemSession.IsOpen);

        _channelMock.Verify(p => p.IsOpen, Times.Exactly(1));
    }

    [Fact]
    public void IsOpenShouldReturnFalseWhenChannelIsNotOpen()
    {
        _channelMock.InSequence(_sequence).Setup(p => p.IsOpen).Returns(false);

        Assert.False(_subsystemSession.IsOpen);

        _channelMock.Verify(p => p.IsOpen, Times.Exactly(1));
    }

    [Fact]
    public void DisposeOnChannelShouldNeverBeInvoked()
    {
        _channelMock.Verify(p => p.Dispose(), Times.Never);
    }
}
