using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CodeBrix.SSH.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortDynamicTest_Start_SessionNotConnected : IDisposable
{
    private Mock<ISession> _sessionMock;
    private Mock<IConnectionInfo> _connectionInfoMock;
    private ForwardedPortDynamic _forwardedPort;
    private IList<EventArgs> _closingRegister;
    private IList<ExceptionEventArgs> _exceptionRegister;
    private SshConnectionException _actualException;
    private IPEndPoint _endpoint;

    public ForwardedPortDynamicTest_Start_SessionNotConnected()
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
            _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(1));
            _forwardedPort.Dispose();
            _forwardedPort = null;
        }
    }

    protected void Arrange()
    {
        _closingRegister = new List<EventArgs>();
        _exceptionRegister = new List<ExceptionEventArgs>();
        _endpoint = new IPEndPoint(IPAddress.Loopback, 8122);

        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);

        _sessionMock.Setup(p => p.IsConnected).Returns(false);
        _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);

        _forwardedPort = new ForwardedPortDynamic(_endpoint.Address.ToString(), (uint)_endpoint.Port);
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
        using (var client = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
            try
            {
                client.Connect(_endpoint);
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
