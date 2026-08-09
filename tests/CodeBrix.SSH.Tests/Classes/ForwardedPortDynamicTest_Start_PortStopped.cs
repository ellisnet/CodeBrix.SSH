using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortDynamicTest_Start_PortStopped : IDisposable
{
    private Mock<ISession> _sessionMock;
    private Mock<IConnectionInfo> _connectionInfoMock;
    private Mock<IChannelDirectTcpip> _channelMock;
    private ForwardedPortDynamic _forwardedPort;
    private IList<EventArgs> _closingRegister;
    private IList<ExceptionEventArgs> _exceptionRegister;
    private IPEndPoint _endpoint;

    public ForwardedPortDynamicTest_Start_PortStopped()
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
        _closingRegister = new List<EventArgs>();
        _exceptionRegister = new List<ExceptionEventArgs>();
        _endpoint = new IPEndPoint(IPAddress.Loopback, 8122);

        _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        _channelMock = new Mock<IChannelDirectTcpip>(MockBehavior.Strict);

        _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(15));
        _sessionMock.Setup(p => p.IsConnected).Returns(true);
        _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
        _sessionMock.Setup(p => p.CreateChannelDirectTcpip()).Returns(_channelMock.Object);

        _forwardedPort = new ForwardedPortDynamic(_endpoint.Address.ToString(), (uint)_endpoint.Port);
        _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
        _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
        _forwardedPort.Session = _sessionMock.Object;
        _forwardedPort.Start();
        _forwardedPort.Stop();

        _closingRegister.Clear();
    }

    protected void Act()
    {
        _forwardedPort.Start();
    }

    [Fact]
    public void IsStartedShouldReturnTrue()
    {
        Assert.True(_forwardedPort.IsStarted);
    }

    [Fact]
    public void ForwardedPortShouldAcceptNewConnections()
    {
        using (var client = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
            client.Connect(_endpoint);
        }
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
