using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.SSH.Abstractions;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes;

public class ShellStreamTest_DisableReadBuffering
{
    private const int BufferSize = 1024;

    private readonly ShellStream _shellStream;
    private readonly ChannelSessionStub _channelSessionStub;

    public ShellStreamTest_DisableReadBuffering()
    {
        _channelSessionStub = new ChannelSessionStub();

        var connectionInfoMock = new Mock<IConnectionInfo>();
        connectionInfoMock.Setup(p => p.Encoding).Returns(Encoding.UTF8);

        var sessionMock = new Mock<ISession>();
        sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        sessionMock.Setup(p => p.ConnectionInfo).Returns(connectionInfoMock.Object);
        sessionMock.Setup(p => p.CreateChannelSession()).Returns(_channelSessionStub);

        _shellStream = new ShellStream(
            sessionMock.Object,
            "terminalName",
            columns: 80,
            rows: 24,
            width: 800,
            height: 600,
            terminalModeValues: null,
            bufferSize: BufferSize);
    }

    [Fact]
    public void DisableReadBuffering_DefaultsToFalse()
    {
        Assert.False(_shellStream.DisableReadBuffering);
    }

    [Fact]
    public void DataReceived_IsStillRaised_WhenReadBufferingDisabled()
    {
        _shellStream.DisableReadBuffering = true;

        byte[] received = null;
        _shellStream.DataReceived += (sender, e) => received = e.Data;

        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello"));

        Assert.Equal(Encoding.UTF8.GetBytes("Hello"), received);
    }

    [Fact]
    public void ReadBuffer_StaysEmpty_WhenReadBufferingDisabled()
    {
        _shellStream.DisableReadBuffering = true;

        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello"));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));

        Assert.False(_shellStream.DataAvailable);
        Assert.Equal(0, _shellStream.Length);
    }

    [Fact]
    public void ReadMethods_Throw_WhenReadBufferingDisabled()
    {
        _shellStream.DisableReadBuffering = true;

        Assert.Throws<InvalidOperationException>(() => _shellStream.Read());
        Assert.Throws<InvalidOperationException>(() => _shellStream.Read(new byte[4], 0, 4));
        Assert.Throws<InvalidOperationException>(() =>
        {
            var buffer = new byte[4];
            _ = _shellStream.Read(buffer.AsSpan());
        });
        Assert.Throws<InvalidOperationException>(() => _shellStream.ReadByte());
        Assert.Throws<InvalidOperationException>(() => _shellStream.ReadLine(TimeSpan.Zero));
        Assert.Throws<InvalidOperationException>(() => _shellStream.Expect("prompt", TimeSpan.Zero));
        Assert.Throws<InvalidOperationException>(() => _shellStream.Expect(new Regex("prompt"), TimeSpan.Zero));
        Assert.Throws<InvalidOperationException>(() => _shellStream.BeginExpect(new ExpectAction(new Regex("prompt"), s => { })));
    }

    [Fact]
    public void EnablingDisableReadBuffering_DiscardsBufferedData()
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("stale data"));

        _shellStream.DisableReadBuffering = true;
        _shellStream.DisableReadBuffering = false;

        Assert.Equal(string.Empty, _shellStream.Read());
    }

    [Fact]
    public void DisablingDisableReadBuffering_ResumesBuffering()
    {
        _shellStream.DisableReadBuffering = true;

        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("dropped"));

        _shellStream.DisableReadBuffering = false;

        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("buffered"));

        Assert.Equal("buffered", _shellStream.Read());
    }

    [Fact]
    public async Task BlockedRead_WakesAndThrows_WhenReadBufferingDisabled()
    {
        Task<int> readTask = _shellStream.ReadAsync(new byte[16], 0, 16, TestContext.Current.CancellationToken);

        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.False(readTask.IsCompleted);

        _shellStream.DisableReadBuffering = true;

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => readTask);
    }

    private class ChannelSessionStub : IChannelSession
    {
        public void Receive(byte[] data)
        {
            DataReceived.Invoke(this, new ChannelDataEventArgs(channelNumber: 0, data));
        }

        public bool SendShellRequest()
        {
            return true;
        }

        public bool SendPseudoTerminalRequest(string environmentVariable, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModeValues)
        {
            return true;
        }

        public void Dispose()
        {
        }

        public void Open()
        {
        }

        public event EventHandler<ChannelDataEventArgs> DataReceived;
#pragma warning disable 0067
        public event EventHandler<ChannelEventArgs> Closed;
        public event EventHandler<ExceptionEventArgs> Exception;
        public event EventHandler<ChannelExtendedDataEventArgs> ExtendedDataReceived;
        public event EventHandler<ChannelRequestEventArgs> RequestReceived;
#pragma warning restore 0067

#pragma warning disable IDE0025 // Use block body for property
#pragma warning disable IDE0022 // Use block body for method
        public uint LocalChannelNumber => throw new NotImplementedException();

        public uint LocalPacketSize => throw new NotImplementedException();

        public uint RemoteChannelNumber => throw new NotImplementedException();

        public uint RemotePacketSize => throw new NotImplementedException();

        public bool IsOpen => throw new NotImplementedException();

        public bool SendBreakRequest(uint breakLength) => throw new NotImplementedException();

        public void SendData(byte[] data) => throw new NotImplementedException();

        public void SendData(byte[] data, int offset, int size) => throw new NotImplementedException();

        public bool SendEndOfWriteRequest() => throw new NotImplementedException();

        public bool SendEnvironmentVariableRequest(string variableName, string variableValue) => throw new NotImplementedException();

        public void SendEof() => throw new NotImplementedException();

        public bool SendExecRequest(string command) => throw new NotImplementedException();

        public bool SendKeepAliveRequest() => throw new NotImplementedException();

        public void SendLocalFlowRequest(bool clientCanDo) => throw new NotImplementedException();

        public bool SendSignalRequest(string signalName) => throw new NotImplementedException();

        public bool SendSubsystemRequest(string subsystem) => throw new NotImplementedException();

        public void SendWindowChangeRequest(uint columns, uint rows, uint width, uint height) => throw new NotImplementedException();

        public bool SendX11ForwardingRequest(bool isSingleConnection, string protocol, byte[] cookie, uint screenNumber) => throw new NotImplementedException();
#pragma warning restore IDE0022 // Use block body for method
#pragma warning restore IDE0025 // Use block body for property
    }
}
