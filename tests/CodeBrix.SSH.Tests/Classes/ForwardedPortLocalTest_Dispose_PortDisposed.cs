using System;
using System.Collections.Generic;
using System.Net;
using CodeBrix.SSH.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortLocalTest_Dispose_PortDisposed : IDisposable
{
    private Mock<ISession> _sessionMock;
    private Mock<IConnectionInfo> _connectionInfoMock;
    private ForwardedPortLocal _forwardedPort;
    private IList<EventArgs> _closingRegister;
    private IList<ExceptionEventArgs> _exceptionRegister;

    public ForwardedPortLocalTest_Dispose_PortDisposed()
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

        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);

        var sequence = new MockSequence();
        _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
        _sessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
        _connectionInfoMock.InSequence(sequence).Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(30));

        _forwardedPort = new ForwardedPortLocal(IPAddress.Loopback.ToString(), "host", 22);
        _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
        _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
        _forwardedPort.Session = _sessionMock.Object;
        _forwardedPort.Start();
        _forwardedPort.Dispose();
    }

    protected void Act()
    {
        _forwardedPort.Dispose();
    }

    [Fact]
    public void ClosingShouldHaveFiredOnce()
    {
        Assert.Single(_closingRegister);
    }

    [Fact]
    public void ExceptionShouldNotHaveFired()
    {
        Assert.Empty(_exceptionRegister);
    }

    [Fact]
    public void SessionShouldBeNull()
    {
        Assert.Null(_forwardedPort.Session);
    }
}
