using System;
using System.Collections.Generic;
using CodeBrix.SSH.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortDynamicTest_Start_PortDisposed : IDisposable
{
    private ForwardedPortDynamic _forwardedPort;
    private IList<EventArgs> _closingRegister;
    private IList<ExceptionEventArgs> _exceptionRegister;
    private ObjectDisposedException _actualException;

    public ForwardedPortDynamicTest_Start_PortDisposed()
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

        _forwardedPort = new ForwardedPortDynamic("host", 22);
        _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
        _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
        _forwardedPort.Dispose();
    }

    protected void Act()
    {
        try
        {
            _forwardedPort.Start();
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ObjectDisposedException ex)
        {
            _actualException = ex;
        }
    }

    [Fact]
    public void StartShouldThrowObjectDisposedException()
    {
        Assert.NotNull(_actualException);
        Assert.Equal(_forwardedPort.GetType().FullName, _actualException.ObjectName);
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
