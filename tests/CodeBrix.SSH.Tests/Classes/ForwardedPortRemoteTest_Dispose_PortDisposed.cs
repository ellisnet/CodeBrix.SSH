using System;
using System.Collections.Generic;
using System.Net;
using CodeBrix.SSH.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortRemoteTest_Dispose_PortDisposed : IDisposable
{
    private ForwardedPortRemote _forwardedPort;
    private IPEndPoint _bindEndpoint;
    private IPEndPoint _remoteEndpoint;
    private IList<EventArgs> _closingRegister;
    private IList<ExceptionEventArgs> _exceptionRegister;

    public ForwardedPortRemoteTest_Dispose_PortDisposed()
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

        _forwardedPort = new ForwardedPortRemote(_bindEndpoint.Address, (uint)_bindEndpoint.Port, _remoteEndpoint.Address, (uint)_remoteEndpoint.Port);
        _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
        _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
        _forwardedPort.Dispose();
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
