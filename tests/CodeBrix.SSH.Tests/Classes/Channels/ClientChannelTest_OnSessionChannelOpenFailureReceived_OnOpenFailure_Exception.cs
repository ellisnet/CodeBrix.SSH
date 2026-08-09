using System;
using System.Collections.Generic;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Channels; //was previously: Renci.SshNet.Tests.Classes.Channels;

public class ClientChannelTest_OnSessionChannelOpenFailureReceived_OnOpenFailure_Exception
{
    private Mock<ISession> _sessionMock;
    private uint _localChannelNumber;
    private uint _localWindowSize;
    private uint _localPacketSize;
    private ClientChannelStub _channel;
    private IList<ExceptionEventArgs> _channelExceptionRegister;
    private Exception _onOpenFailureException;

    public ClientChannelTest_OnSessionChannelOpenFailureReceived_OnOpenFailure_Exception()
    {
        Initialize();
    }

    private void Initialize()
    {
        Arrange();
        Act();
    }

    private void Arrange()
    {
        var random = new Random();
        _localChannelNumber = (uint)random.Next(0, int.MaxValue);
        _localWindowSize = (uint)random.Next(1000, int.MaxValue);
        _localPacketSize = _localWindowSize - 1;
        _onOpenFailureException = new SystemException();
        _channelExceptionRegister = new List<ExceptionEventArgs>();

        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);

        _channel = new ClientChannelStub(_sessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
        _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
        _channel.OnOpenFailureException = _onOpenFailureException;
    }

    private void Act()
    {
        _sessionMock.Raise(s => s.ChannelOpenFailureReceived += null,
            new MessageEventArgs<ChannelOpenFailureMessage>(
                new ChannelOpenFailureMessage(_localChannelNumber, "", ChannelOpenFailureMessage.ConnectFailed)));
    }

    [Fact]
    public void ExceptionEventShouldHaveFiredOnce()
    {
        Assert.Single(_channelExceptionRegister);
        Assert.Same(_onOpenFailureException, _channelExceptionRegister[0].Exception);
    }

    [Fact]
    public void OnErrorOccurredShouldBeInvokedOnce()
    {
        Assert.Single(_channel.OnErrorOccurredInvocations);
        Assert.Same(_onOpenFailureException, _channel.OnErrorOccurredInvocations[0]);
    }
}
