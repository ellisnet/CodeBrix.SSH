using System;
using System.Collections.Generic;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Channels; //was previously: Renci.SshNet.Tests.Classes.Channels;

public class ChannelTest_OnSessionChannelCloseReceived_OnClose_Exception : ChannelTestBase
{
    private uint _localWindowSize;
    private uint _localPacketSize;
    private uint _localChannelNumber;
    private ChannelStub _channel;
    private IList<ExceptionEventArgs> _channelExceptionRegister;
    private Exception _onCloseException;

    protected override void SetupData()
    {
        var random = new Random();

        _localWindowSize = (uint)random.Next(1000, int.MaxValue);
        _localPacketSize = _localWindowSize - 1;
        _localChannelNumber = (uint)random.Next(0, int.MaxValue);
        _onCloseException = new SystemException();
        _channelExceptionRegister = new List<ExceptionEventArgs>();
    }

    protected override void SetupMocks()
    {
    }

    protected override void Arrange()
    {
        base.Arrange();

        _channel = new ChannelStub(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
        _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
        _channel.OnCloseException = _onCloseException;
    }

    protected override void Act()
    {
        SessionMock.Raise(s => s.ChannelCloseReceived += null,
            new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
    }

    [Fact]
    public void ExceptionEventShouldHaveFiredOnce()
    {
        Assert.Single(_channelExceptionRegister);
        Assert.Same(_onCloseException, _channelExceptionRegister[0].Exception);
    }

    [Fact]
    public void OnErrorOccurredShouldBeInvokedOnce()
    {
        Assert.Single(_channel.OnErrorOccurredInvocations);
        Assert.Same(_onCloseException, _channel.OnErrorOccurredInvocations[0]);
    }
}
