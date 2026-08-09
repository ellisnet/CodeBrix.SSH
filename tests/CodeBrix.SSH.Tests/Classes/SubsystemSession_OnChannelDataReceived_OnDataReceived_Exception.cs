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

public class SubsystemSession_OnChannelDataReceived_OnDataReceived_Exception
{
    private Mock<ISession> _sessionMock;
    private Mock<IChannelSession> _channelMock;
    private string _subsystemName;
    private SubsystemSessionStub _subsystemSession;
    private int _operationTimeout;
    private IList<EventArgs> _disconnectedRegister;
    private IList<ExceptionEventArgs> _errorOccurredRegister;
    private ChannelDataEventArgs _channelDataEventArgs;
    private Exception _onDataReceivedException;

    public SubsystemSession_OnChannelDataReceived_OnDataReceived_Exception()
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
        _channelDataEventArgs = new ChannelDataEventArgs(
            (uint)random.Next(0, int.MaxValue),
            new[] { (byte)random.Next(byte.MinValue, byte.MaxValue) });
        _onDataReceivedException = new SystemException();

        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        _channelMock = new Mock<IChannelSession>(MockBehavior.Strict);

        var sequence = new MockSequence();
        _sessionMock.InSequence(sequence).Setup(p => p.CreateChannelSession()).Returns(_channelMock.Object);
        _channelMock.InSequence(sequence).Setup(p => p.Open());
        _channelMock.InSequence(sequence).Setup(p => p.SendSubsystemRequest(_subsystemName)).Returns(true);

        _subsystemSession = new SubsystemSessionStub(
            _sessionMock.Object,
            _subsystemName,
            _operationTimeout);
        _subsystemSession.Disconnected += (sender, args) => _disconnectedRegister.Add(args);
        _subsystemSession.ErrorOccurred += (sender, args) => _errorOccurredRegister.Add(args);
        _subsystemSession.OnDataReceivedException = _onDataReceivedException;
        _subsystemSession.Connect();
    }

    protected void Act()
    {
        _channelMock.Raise(s => s.DataReceived += null, _channelDataEventArgs);
    }

    [Fact]
    public void DisconnectHasNeverFired()
    {
        Assert.Empty(_disconnectedRegister);
    }

    [Fact]
    public void ErrorOccurredHaveFiredOnce()
    {
        _errorOccurredRegister.Count.Should().Be(1, _errorOccurredRegister.AsString());
        _errorOccurredRegister[0].Exception.Should().BeSameAs(_onDataReceivedException, _errorOccurredRegister.AsString());
    }

    [Fact]
    public void OnDataReceivedShouldBeInvokedOnce()
    {
        Assert.Single(_subsystemSession.OnDataReceivedInvocations);

        var received = _subsystemSession.OnDataReceivedInvocations[0];
        Assert.Equal(_channelDataEventArgs.Data, received.Data);
    }
}
