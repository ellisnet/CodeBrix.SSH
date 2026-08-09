using System;
using System.Collections.Generic;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Channels; //was previously: Renci.SshNet.Tests.Classes.Channels;

public class ChannelTest_OnSessionChannelRequestReceived_OnRequest_Exception : ChannelTestBase
{
    private uint _localWindowSize;
    private uint _localPacketSize;
    private uint _localChannelNumber;
    private ChannelStub _channel;
    private IList<ExceptionEventArgs> _channelExceptionRegister;
    private Exception _onRequestException;
    private SignalRequestInfo _requestInfo;

    protected override void SetupData()
    {
        var random = new Random();

        _localWindowSize = (uint)random.Next(1000, int.MaxValue);
        _localPacketSize = _localWindowSize - 1;
        _localChannelNumber = (uint)random.Next(0, int.MaxValue);
        _onRequestException = new SystemException();
        _channelExceptionRegister = new List<ExceptionEventArgs>();
        _requestInfo = new SignalRequestInfo("ABC");
    }

    protected override void SetupMocks()
    {
        _ = SessionMock.Setup(p => p.ConnectionInfo)
                       .Returns(new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "password")));
    }

    protected override void Arrange()
    {
        base.Arrange();

        _channel = new ChannelStub(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
        _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
        _channel.OnRequestException = _onRequestException;
    }

    protected override void Act()
    {
        SessionMock.Raise(s => s.ChannelRequestReceived += null,
                           new MessageEventArgs<ChannelRequestMessage>(new ChannelRequestMessage(_localChannelNumber, _requestInfo)));
    }

    [Fact]
    public void ExceptionEventShouldHaveFiredOnce()
    {
        Assert.Single(_channelExceptionRegister);
        Assert.Same(_onRequestException, _channelExceptionRegister[0].Exception);
    }

    [Fact]
    public void OnErrorOccurredShouldBeInvokedOnce()
    {
        Assert.Single(_channel.OnErrorOccurredInvocations);
        Assert.Same(_onRequestException, _channel.OnErrorOccurredInvocations[0]);
    }
}
