using System;
using System.Collections.Generic;
using System.Globalization;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SubsystemSession_Disconnect_Connected
{
    private Mock<ISession> _sessionMock;
    private Mock<IChannelSession> _channelMock;
    private string _subsystemName;
    private SubsystemSessionStub _subsystemSession;
    private int _operationTimeout;
    private IList<EventArgs> _disconnectedRegister;
    private IList<ExceptionEventArgs> _errorOccurredRegister;
    private MockSequence _sequence;

    public SubsystemSession_Disconnect_Connected()
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
        _channelMock = new Mock<IChannelSession>(MockBehavior.Strict);

        _sequence = new MockSequence();

        _ = _sessionMock.InSequence(_sequence)
                        .Setup(p => p.CreateChannelSession())
                        .Returns(_channelMock.Object);
        _ = _channelMock.InSequence(_sequence)
                        .Setup(p => p.Open());
        _ = _channelMock.InSequence(_sequence)
                        .Setup(p => p.SendSubsystemRequest(_subsystemName))
                        .Returns(true);
        _ = _channelMock.InSequence(_sequence)
                        .Setup(p => p.Dispose());

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
        _subsystemSession.Disconnect();
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
    public void IsOpenShouldReturnFalse()
    {
        Assert.False(_subsystemSession.IsOpen);
    }

    [Fact]
    public void DisposeOnChannelShouldBeInvokedOnce()
    {
        _channelMock.Verify(p => p.Dispose(), Times.Once);
    }
}
