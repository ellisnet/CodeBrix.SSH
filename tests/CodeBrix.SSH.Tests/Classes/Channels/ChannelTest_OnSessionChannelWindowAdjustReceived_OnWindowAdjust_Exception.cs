using System;
using System.Collections.Generic;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Channels; //was previously: Renci.SshNet.Tests.Classes.Channels;

public class ChannelTest_OnSessionChannelWindowAdjustReceived_OnWindowAdjust_Exception : ChannelTestBase
{
    private uint _localChannelNumber;
    private uint _localWindowSize;
    private uint _localPacketSize;
    private uint _remoteChannelNumber;
    private uint _remoteWindowSize;
    private uint _remotePacketSize;
    private ChannelStub _channel;
    private IList<ExceptionEventArgs> _channelExceptionRegister;
    private Exception _onWindowAdjustException;
    private uint _bytesToAdd;

    protected override void SetupData()
    {
        var random = new Random();

        _localChannelNumber = (uint)random.Next(0, int.MaxValue);
        _localWindowSize = (uint)random.Next(1000, int.MaxValue);
        _localPacketSize = _localWindowSize - 1;
        _remoteChannelNumber = (uint)random.Next(0, int.MaxValue);
        _remoteWindowSize = (uint)random.Next(1000, int.MaxValue);
        _remotePacketSize = _localWindowSize - 1;
        _bytesToAdd = (uint)random.Next(0, int.MaxValue);
        _onWindowAdjustException = new SystemException();
        _channelExceptionRegister = new List<ExceptionEventArgs>();
    }

    protected override void SetupMocks()
    {
    }

    protected override void Arrange()
    {
        base.Arrange();

        _channel = new ChannelStub(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
        _channel.InitializeRemoteChannelInfo(_remoteChannelNumber, _remoteWindowSize, _remotePacketSize);
        _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
        _channel.OnWindowAdjustException = _onWindowAdjustException;
    }

    protected override void Act()
    {
        SessionMock.Raise(s => s.ChannelWindowAdjustReceived += null,
                           new MessageEventArgs<ChannelWindowAdjustMessage>(new ChannelWindowAdjustMessage(_localChannelNumber, _bytesToAdd)));
    }

    [Fact]
    public void ExceptionEventShouldHaveFiredOnce()
    {
        Assert.Single(_channelExceptionRegister);
        Assert.Same(_onWindowAdjustException, _channelExceptionRegister[0].Exception);
    }

    [Fact]
    public void OnErrorOccurredShouldBeInvokedOnce()
    {
        Assert.Single(_channel.OnErrorOccurredInvocations);
        Assert.Same(_onWindowAdjustException, _channel.OnErrorOccurredInvocations[0]);
    }
}
