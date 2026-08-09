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

public class ForwardedPortRemoteTest_Start_SessionNotConnected : IDisposable
{
    private Mock<ISession> _sessionMock;
    private Mock<IConnectionInfo> _connectionInfoMock;
    private ForwardedPortRemote _forwardedPort;
    private IPEndPoint _bindEndpoint;
    private IPEndPoint _remoteEndpoint;
    private IList<EventArgs> _closingRegister;
    private IList<ExceptionEventArgs> _exceptionRegister;
    private SshConnectionException _actualException;

    public ForwardedPortRemoteTest_Start_SessionNotConnected()
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
            _ = _sessionMock.Setup(p => p.ConnectionInfo)
                            .Returns(_connectionInfoMock.Object);
            _ = _connectionInfoMock.Setup(p => p.Timeout)
                                   .Returns(TimeSpan.FromSeconds(1));

            _forwardedPort.Dispose();
            _forwardedPort = null;
        }
    }

    protected void Arrange()
    {
        var random = new Random();
        _closingRegister = new List<EventArgs>();
        _exceptionRegister = new List<ExceptionEventArgs>();
        _bindEndpoint = new IPEndPoint(IPAddress.Any, random.Next(IPEndPoint.MinPort, 1000));
        _remoteEndpoint = new IPEndPoint(IPAddress.Parse("193.168.1.5"), random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));

        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);

        _ = _sessionMock.Setup(p => p.IsConnected)
                        .Returns(false);

        _forwardedPort = new ForwardedPortRemote(_bindEndpoint.Address, (uint)_bindEndpoint.Port, _remoteEndpoint.Address, (uint)_remoteEndpoint.Port);
        _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
        _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
        _forwardedPort.Session = _sessionMock.Object;
    }

    protected void Act()
    {
        try
        {
            _forwardedPort.Start();
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshConnectionException ex)
        {
            _actualException = ex;
        }
    }

    [Fact]
    public void StartShouldThrowSshConnectionException()
    {
        Assert.NotNull(_actualException);
        Assert.Equal("Client not connected.", _actualException.Message);
    }

    [Fact]
    public void IsStartedShouldReturnFalse()
    {
        Assert.False(_forwardedPort.IsStarted);
    }

    [Fact]
    public void ForwardedPortShouldIgnoreReceivedSignalForNewConnection()
    {
        var channelNumber = (uint)new Random().Next(1001, int.MaxValue);
        var initialWindowSize = (uint)new Random().Next(0, int.MaxValue);
        var maximumPacketSize = (uint)new Random().Next(0, int.MaxValue);
        var originatorAddress = new Random().Next().ToString(CultureInfo.InvariantCulture);
        var originatorPort = (uint)new Random().Next(0, int.MaxValue);
        var channelMock = new Mock<IChannelForwardedTcpip>(MockBehavior.Strict);

        _ = _sessionMock.Setup(p => p.CreateChannelForwardedTcpip(channelNumber, initialWindowSize, maximumPacketSize)).Returns(channelMock.Object);

        _sessionMock.Raise(p => p.ChannelOpenReceived += null,
                new MessageEventArgs<ChannelOpenMessage>(new ChannelOpenMessage(channelNumber, initialWindowSize,
                maximumPacketSize,
                new ForwardedTcpipChannelInfo(_forwardedPort.BoundHost, _forwardedPort.BoundPort, originatorAddress,
                    originatorPort))));

        _sessionMock.Verify(p => p.CreateChannelForwardedTcpip(channelNumber, initialWindowSize, maximumPacketSize), Times.Never);
        _sessionMock.Verify(p => p.SendMessage(new ChannelOpenFailureMessage(channelNumber, string.Empty, ChannelOpenFailureMessage.AdministrativelyProhibited)), Times.Never);
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
