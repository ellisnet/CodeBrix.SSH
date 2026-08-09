using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CodeBrix.SSH.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortDynamicTest_Start_SessionNull : IDisposable
{
    private ForwardedPortDynamic _forwardedPort;
    private IList<EventArgs> _closingRegister;
    private IList<ExceptionEventArgs> _exceptionRegister;
    private InvalidOperationException _actualException;
    private IPEndPoint _endpoint;

    public ForwardedPortDynamicTest_Start_SessionNull()
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

        _forwardedPort = new ForwardedPortDynamic(_endpoint.Address.ToString(), (uint)_endpoint.Port);
        _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
        _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
    }

    protected void Act()
    {
        try
        {
            _forwardedPort.Start();
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (InvalidOperationException ex)
        {
            _actualException = ex;
        }
    }

    [Fact]
    public void StartShouldThrowInvalidOperationException()
    {
        Assert.NotNull(_actualException);
        Assert.Equal("Forwarded port is not added to a client.", _actualException.Message);
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
