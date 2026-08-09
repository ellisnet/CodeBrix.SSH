using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortLocalTest_Dispose_PortStarted_ChannelBound : IDisposable
{
    private Mock<ISession> _sessionMock;
    private Mock<IConnectionInfo> _connectionInfoMock;
    private Mock<IChannelDirectTcpip> _channelMock;
    private ForwardedPortLocal _forwardedPort;
    private IList<EventArgs> _closingRegister;
    private IList<ExceptionEventArgs> _exceptionRegister;
    private IPEndPoint _localEndpoint;
    private IPEndPoint _remoteEndpoint;
    private Socket _client;
    private TimeSpan _bindSleepTime;
    private ManualResetEvent _channelBindStarted;
    private ManualResetEvent _channelBindCompleted;

    public ForwardedPortLocalTest_Dispose_PortStarted_ChannelBound()
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
        if (_client != null)
        {
            _client.Dispose();
            _client = null;
        }
        if (_forwardedPort != null)
        {
            _forwardedPort.Dispose();
            _forwardedPort = null;
        }
        if (_channelBindStarted != null)
        {
            _channelBindStarted.Dispose();
            _channelBindStarted = null;
        }
        if (_channelBindCompleted != null)
        {
            _channelBindCompleted.Dispose();
            _channelBindCompleted = null;
        }
    }

    protected void Arrange()
    {
        var random = new Random();
        _closingRegister = new List<EventArgs>();
        _exceptionRegister = new List<ExceptionEventArgs>();
        _localEndpoint = new IPEndPoint(IPAddress.Loopback, 8122);
        _remoteEndpoint = new IPEndPoint(IPAddress.Parse("193.168.1.5"), random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));
        _bindSleepTime = TimeSpan.FromMilliseconds(random.Next(100, 500));
        _forwardedPort = new ForwardedPortLocal(_localEndpoint.Address.ToString(), (uint)_localEndpoint.Port, _remoteEndpoint.Address.ToString(), (uint)_remoteEndpoint.Port);
        _channelBindStarted = new ManualResetEvent(false);
        _channelBindCompleted = new ManualResetEvent(false);

        _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        _channelMock = new Mock<IChannelDirectTcpip>(MockBehavior.Strict);

        _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(15));
        _sessionMock.Setup(p => p.IsConnected).Returns(true);
        _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
        _sessionMock.Setup(p => p.CreateChannelDirectTcpip()).Returns(_channelMock.Object);
        _channelMock.Setup(p => p.Open(_forwardedPort.Host, _forwardedPort.Port, _forwardedPort, It.IsAny<Socket>()));
        _channelMock.Setup(p => p.Bind()).Callback(() =>
            {
                _channelBindStarted.Set();
                Thread.Sleep(_bindSleepTime);
                _channelBindCompleted.Set();
            });
        _channelMock.Setup(p => p.Dispose());

        _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
        _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
        _forwardedPort.Session = _sessionMock.Object;
        _forwardedPort.Start();

        _client = new Socket(_localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            ReceiveTimeout = 100,
            SendTimeout = 500,
            SendBufferSize = 0
        };

        _client.Connect(_localEndpoint);

        // wait for SOCKS client to bind to channel
        Assert.True(_channelBindStarted.WaitOne(TimeSpan.FromMilliseconds(200)));
    }

    protected void Act()
    {
        _forwardedPort.Dispose();
    }

    [Fact]
    public void ShouldBlockUntilBindHasCompleted()
    {
        Assert.True(_channelBindCompleted.WaitOne(0));
    }

    [Fact]
    public void IsStartedShouldReturnFalse()
    {
        Assert.False(_forwardedPort.IsStarted);
    }

    [Fact]
    public void ForwardedPortShouldRefuseNewConnections()
    {
        using (var client = new Socket(_localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
            try
            {
                client.Connect(_localEndpoint);
                Assert.Fail("Test failed: reached code that should not have been reached.");
            }
            catch (SocketException ex)
            {
                Assert.Equal(SocketError.ConnectionRefused, ex.SocketErrorCode);
            }
        }
    }

    [Fact]
    public void BoundClientShouldNotBeClosed()
    {
        // the forwarded port itself does not close the client connection; when the channel is closed properly
        // it's the channel that will take care of closing the client connection
        //
        // we'll check if the client connection is still alive by attempting to receive, which should time out
        // as the forwarded port (or its channel) are not sending anything

        var buffer = new byte[1];

        try
        {
            _client.Receive(buffer);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SocketException ex)
        {
            Assert.Equal(SocketError.TimedOut, ex.SocketErrorCode);
        }
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
    public void OpenOnChannelShouldBeInvokedOnce()
    {
        _channelMock.Verify(p => p.Open(_forwardedPort.Host, _forwardedPort.Port, _forwardedPort, It.IsAny<Socket>()), Times.Once);
    }

    [Fact]
    public void BindOnChannelShouldBeInvokedOnce()
    {
        _channelMock.Verify(p => p.Bind(), Times.Once);
    }

    [Fact]
    public void DisposeOnChannelShouldBeInvokedOnce()
    {
        _channelMock.Verify(p => p.Dispose(), Times.Once);
    }
}
