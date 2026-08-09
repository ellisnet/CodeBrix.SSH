using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortRemoteTest_Dispose_PortNeverStarted : IDisposable
{
    private Mock<ISession> _sessionMock;
    private Mock<IConnectionInfo> _connectionInfoMock;
    private ForwardedPortRemote _forwardedPort;
    private IList<EventArgs> _closingRegister;
    private IList<ExceptionEventArgs> _exceptionRegister;
    private IPEndPoint _bindEndpoint;
    private IPEndPoint _remoteEndpoint;

    public ForwardedPortRemoteTest_Dispose_PortNeverStarted()
    {
        Setup();
    }

    private void Setup()
    {
        Arrange();
        Act();
    }

    public void Dispose()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        if (_forwardedPort != null)
        {
            _forwardedPort.Dispose();
            _forwardedPort = null;
        }
    }

    protected void Arrange()
    {
        var random = new Random();

        _closingRegister = new List<EventArgs>();
        _exceptionRegister = new List<ExceptionEventArgs>();
        _bindEndpoint = new IPEndPoint(IPAddress.Any, random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));
        _remoteEndpoint = new IPEndPoint(IPAddress.Parse("193.168.1.5"), random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));

        _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);

        _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(15));
        _sessionMock.Setup(p => p.IsConnected).Returns(true);
        _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);

        _forwardedPort = new ForwardedPortRemote(_bindEndpoint.Address, (uint)_bindEndpoint.Port, _remoteEndpoint.Address, (uint)_remoteEndpoint.Port);
        _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
        _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
        _forwardedPort.Session = _sessionMock.Object;
    }

    protected void Act()
    {
        _forwardedPort.Dispose();
    }

    [Fact]
    public void IsStartedShouldReturnFalse()
    {
        Assert.False(_forwardedPort.IsStarted);
    }

    [Fact]
    public void ForwardedPortShouldIgnoreChannelOpenMessagesWhenDisposed()
    {
        var channelNumberDisposed = (uint)new Random().Next(1001, int.MaxValue);
        var initialWindowSizeDisposed = (uint)new Random().Next(0, int.MaxValue);
        var maximumPacketSizeDisposed = (uint)new Random().Next(0, int.MaxValue);
        var originatorAddressDisposed = new Random().Next().ToString(CultureInfo.InvariantCulture);
        var originatorPortDisposed = (uint)new Random().Next(0, int.MaxValue);
        var channelMock = new Mock<IChannelForwardedTcpip>(MockBehavior.Strict);

        _sessionMock.Setup(
            p =>
                p.CreateChannelForwardedTcpip(channelNumberDisposed, initialWindowSizeDisposed, maximumPacketSizeDisposed)).Returns(channelMock.Object);
        _sessionMock.Setup(
            p =>
                p.SendMessage(new ChannelOpenFailureMessage(channelNumberDisposed, string.Empty,
                    ChannelOpenFailureMessage.AdministrativelyProhibited)));

        _sessionMock.Raise(p => p.ChannelOpenReceived += null, new ChannelOpenMessage(channelNumberDisposed, initialWindowSizeDisposed, maximumPacketSizeDisposed, new ForwardedTcpipChannelInfo(_forwardedPort.BoundHost, _forwardedPort.BoundPort, originatorAddressDisposed, originatorPortDisposed)));

        _sessionMock.Verify(p => p.CreateChannelForwardedTcpip(channelNumberDisposed, initialWindowSizeDisposed, maximumPacketSizeDisposed), Times.Never);
        _sessionMock.Verify(p => p.SendMessage(It.Is<ChannelOpenFailureMessage>(c => c.LocalChannelNumber == channelNumberDisposed && c.ReasonCode == ChannelOpenFailureMessage.AdministrativelyProhibited && c.Description == string.Empty && c.Language == null)), Times.Never);
    }

    [Fact]
    public void ClosingShouldNotHaveFired()
    {
        Assert.Empty(_closingRegister);
    }

    [Fact]
    public void ExceptionShouldNotHaveFired()
    {
        Assert.Empty(_exceptionRegister);
    }
}
