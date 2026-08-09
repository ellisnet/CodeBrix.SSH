using System;
using System.Collections.Generic;
using System.Globalization;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SubsystemSession_Connect_Disconnected
{
    private Mock<ISession> _sessionMock;
    private Mock<IChannelSession> _channelBeforeDisconnectMock;
    private Mock<IChannelSession> _channelAfterDisconnectMock;
    private string _subsystemName;
    private SubsystemSessionStub _subsystemSession;
    private int _operationTimeout;
    private IList<EventArgs> _disconnectedRegister;
    private IList<ExceptionEventArgs> _errorOccurredRegister;
    private MockSequence _sequence;

    public SubsystemSession_Connect_Disconnected()
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

        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        _channelBeforeDisconnectMock = new Mock<IChannelSession>(MockBehavior.Strict);
        _channelAfterDisconnectMock = new Mock<IChannelSession>(MockBehavior.Strict);

        _sequence = new MockSequence();

        _ = _sessionMock.InSequence(_sequence)
                        .Setup(p => p.CreateChannelSession())
                        .Returns(_channelBeforeDisconnectMock.Object);
        _ = _channelBeforeDisconnectMock.InSequence(_sequence)
                                        .Setup(p => p.Open());
        _ = _channelBeforeDisconnectMock.InSequence(_sequence)
                                        .Setup(p => p.SendSubsystemRequest(_subsystemName))
                                        .Returns(true);
        _ = _channelBeforeDisconnectMock.InSequence(_sequence)
                                        .Setup(p => p.Dispose());
        _ = _sessionMock.InSequence(_sequence)
                        .Setup(p => p.CreateChannelSession())
                        .Returns(_channelAfterDisconnectMock.Object);
        _ = _channelAfterDisconnectMock.InSequence(_sequence)
                                       .Setup(p => p.Open());
        _ = _channelAfterDisconnectMock.InSequence(_sequence)
                                       .Setup(p => p.SendSubsystemRequest(_subsystemName))
                                       .Returns(true);

        _subsystemSession = new SubsystemSessionStub(_sessionMock.Object,
                                                     _subsystemName,
                                                     _operationTimeout);
        _subsystemSession.Disconnected += (sender, args) => _disconnectedRegister.Add(args);
        _subsystemSession.ErrorOccurred += (sender, args) => _errorOccurredRegister.Add(args);
        _subsystemSession.Connect();
        _subsystemSession.Disconnect();
    }

    protected void Act()
    {
        _subsystemSession.Connect();
    }

    [Fact]
    public void DisconnectHasNeverFired()
    {
        Assert.Empty(_disconnectedRegister);
    }

    [Fact]
    public void ErrorOccurredHasNeverFired()
    {
        Assert.Empty(_errorOccurredRegister);
    }

    [Fact]
    public void IsOpenShouldReturnTrueWhenChannelIsOpen()
    {
        _ = _channelAfterDisconnectMock.InSequence(_sequence)
                                       .Setup(p => p.IsOpen)
                                       .Returns(true);

        Assert.True(_subsystemSession.IsOpen);

        _channelAfterDisconnectMock.Verify(p => p.IsOpen, Times.Once);
    }

    [Fact]
    public void IsOpenShouldReturnFalseWhenChannelIsNotOpen()
    {
        _ = _channelAfterDisconnectMock.InSequence(_sequence)
                                       .Setup(p => p.IsOpen)
                                       .Returns(false);

        Assert.False(_subsystemSession.IsOpen);

        _channelAfterDisconnectMock.Verify(p => p.IsOpen, Times.Once);
    }

    [Fact]
    public void DisposeOnChannelBeforeDisconnectShouldBeInvokedOnce()
    {
        _channelBeforeDisconnectMock.Verify(p => p.Dispose(), Times.Once);
    }

    [Fact]
    public void DisposeOnChannelAfterDisconnectShouldNeverBeInvoked()
    {
        _channelAfterDisconnectMock.Verify(p => p.Dispose(), Times.Never);
    }
}
