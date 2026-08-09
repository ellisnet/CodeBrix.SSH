using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CodeBrix.SSH.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortLocalTest_Start_SessionNotConnected : IDisposable
{
    private Mock<ISession> _sessionMock;
    private Mock<IConnectionInfo> _connectionInfoMock;
    private ForwardedPortLocal _forwardedPort;
    private IPEndPoint _localEndpoint;
    private IPEndPoint _remoteEndpoint;
    private IList<EventArgs> _closingRegister;
    private IList<ExceptionEventArgs> _exceptionRegister;
    private SshConnectionException _actualException;

    public ForwardedPortLocalTest_Start_SessionNotConnected()
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
            _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(1));
            _forwardedPort.Dispose();
            _forwardedPort = null;
        }
    }

    protected void Arrange()
    {
        var random = new Random();
        _closingRegister = new List<EventArgs>();
        _exceptionRegister = new List<ExceptionEventArgs>();
        _localEndpoint = new IPEndPoint(IPAddress.Loopback, 8122);
        _remoteEndpoint = new IPEndPoint(IPAddress.Parse("193.168.1.5"),
            random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));

        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);

        _sessionMock.Setup(p => p.IsConnected).Returns(false);

        _forwardedPort = new ForwardedPortLocal(_localEndpoint.Address.ToString(), (uint)_localEndpoint.Port,
            _remoteEndpoint.Address.ToString(), (uint)_remoteEndpoint.Port);
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
    public void ForwardedPortShouldRejectNewConnections()
    {
        using (var client = new Socket(_localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
            try
            {
                client.Connect(_localEndpoint);
            }
            catch (SocketException ex)
            {
                Assert.Equal(SocketError.ConnectionRefused, ex.SocketErrorCode);
            }
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
