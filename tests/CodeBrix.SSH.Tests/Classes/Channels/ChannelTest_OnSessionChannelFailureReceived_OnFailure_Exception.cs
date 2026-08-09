using System;
using System.Collections.Generic;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Channels; //was previously: Renci.SshNet.Tests.Classes.Channels;

public class ChannelTest_OnSessionChannelFailureReceived_OnFailure_Exception : ChannelTestBase
{
    private uint _localWindowSize;
    private uint _localPacketSize;
    private uint _localChannelNumber;
    private ChannelStub _channel;
    private IList<ExceptionEventArgs> _channelExceptionRegister;
    private Exception _onFailureException;

    protected override void SetupData()
    {
        var random = new Random();

        _localWindowSize = (uint)random.Next(0, 1000);
        _localPacketSize = (uint)random.Next(1001, int.MaxValue);
        _localChannelNumber = (uint)random.Next(0, int.MaxValue);
        _onFailureException = new SystemException();
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
        _channel.OnFailureException = _onFailureException;
    }

    protected override void Act()
    {
        SessionMock.Raise(s => s.ChannelFailureReceived += null,
                           new MessageEventArgs<ChannelFailureMessage>(new ChannelFailureMessage(_localChannelNumber)));
    }

    [Fact]
    public void ExceptionEventShouldHaveFiredOnce()
    {
        Assert.Single(_channelExceptionRegister);
        Assert.Same(_onFailureException, _channelExceptionRegister[0].Exception);
    }

    [Fact]
    public void OnErrorOccurredShouldBeInvokedOnce()
    {
        Assert.Single(_channel.OnErrorOccurredInvocations);
        Assert.Same(_onFailureException, _channel.OnErrorOccurredInvocations[0]);
    }
}
