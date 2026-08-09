using System;
using System.Collections.Generic;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Channels; //was previously: Renci.SshNet.Tests.Classes.Channels;

public class ChannelTest_Dispose_SessionIsNotConnectedAndChannelIsOpen : ChannelTestBase
{
    private uint _localWindowSize;
    private uint _localPacketSize;
    private uint _localChannelNumber;
    private ChannelStub _channel;
    private List<ChannelEventArgs> _channelClosedRegister;
    private IList<ExceptionEventArgs> _channelExceptionRegister;

    protected override void SetupData()
    {
        var random = new Random();

        _localChannelNumber = (uint)random.Next(0, int.MaxValue);
        _localWindowSize = (uint)random.Next(0, int.MaxValue);
        _localPacketSize = (uint)random.Next(0, int.MaxValue);
        _channelClosedRegister = new List<ChannelEventArgs>();
        _channelExceptionRegister = new List<ExceptionEventArgs>();
    }

    protected override void SetupMocks()
    {
        SessionMock.Setup(p => p.IsConnected).Returns(false);
    }

    protected override void Arrange()
    {
        base.Arrange();

        _channel = new ChannelStub(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
        _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
        _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
        _channel.SetIsOpen(true);
    }

    protected override void Act()
    {
        _channel.Dispose();
    }

    [Fact]
    public void IsOpenShouldReturnFalse()
    {
        Assert.False(_channel.IsOpen);
    }

    [Fact]
    public void SendMessageOnSessionShouldNeverBeInvoked()
    {
        SessionMock.Verify(p => p.SendMessage(It.IsAny<Message>()), Times.Never);
    }

    [Fact]
    public void ClosedEventShouldNeverHaveFired()
    {
        Assert.Empty(_channelClosedRegister);
    }

    [Fact]
    public void ExceptionShouldNeverHaveFired()
    {
        Assert.Empty(_channelExceptionRegister);
    }
}
