using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.SSH.Abstractions;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ShellStreamTest_ReadExpect
{
    private const int BufferSize = 1024;
    private ShellStream _shellStream;
    private ChannelSessionStub _channelSessionStub;

    public ShellStreamTest_ReadExpect()
    {
        Initialize();
    }

    private void Initialize()
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
    public void Read_String()
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));

        Assert.Equal("Hello World!", _shellStream.Read());
    }

    [Fact]
    public void Read_Bytes()
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));

        byte[] buffer = new byte[12];

        Assert.Equal(7, _shellStream.Read(buffer, 3, 7));
        Assert.Equal(Encoding.UTF8.GetBytes("\0\0\0Hello W\0\0"), buffer);

        Assert.Equal(5, _shellStream.Read(buffer, 0, 12));
        Assert.Equal(Encoding.UTF8.GetBytes("orld!llo W\0\0"), buffer);
    }

    [Fact]
    public void Read_Bytes_Span()
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));

        byte[] buffer = new byte[12];

        Assert.Equal(7, _shellStream.Read(buffer.AsSpan(3, 7)));
        Assert.Equal(Encoding.UTF8.GetBytes("\0\0\0Hello W\0\0"), buffer);

        Assert.Equal(5, _shellStream.Read(buffer));
        Assert.Equal(Encoding.UTF8.GetBytes("orld!llo W\0\0"), buffer);
    }

    [Fact]
    public void Channel_DataReceived_MoreThanBufferSize()
    {
        // Test buffer resizing
        byte[] expectedData = CryptoAbstraction.GenerateRandom(BufferSize * 3);
        _channelSessionStub.Receive(expectedData);

        byte[] actualData = new byte[expectedData.Length + 1];

        Assert.Equal(expectedData.Length, _shellStream.Read(actualData, 0, actualData.Length));
        Assert.Equal(expectedData, actualData.Take(expectedData.Length));
    }

    [Theory]
    [InlineData("\r\n")]
    [InlineData("\r")]
    [InlineData("\n")]
    public void ReadLine(string newLine)
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));

        // We specify a timeout to avoid waiting infinitely.
        Assert.Null(_shellStream.ReadLine(TimeSpan.Zero));

        _channelSessionStub.Receive(Encoding.UTF8.GetBytes(newLine));

        Assert.Equal("Hello World!", _shellStream.ReadLine(TimeSpan.Zero));
        Assert.Null(_shellStream.ReadLine(TimeSpan.Zero));

        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Second line!" + newLine + "Third line!" + newLine));

        Assert.Equal("Second line!", _shellStream.ReadLine(TimeSpan.Zero));
        Assert.Equal("Third line!", _shellStream.ReadLine(TimeSpan.Zero));
        Assert.Null(_shellStream.ReadLine(TimeSpan.Zero));

        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Last line!")); // no newLine at the end

        Assert.Null(_shellStream.ReadLine(TimeSpan.Zero));

        _channelSessionStub.Close();

        Assert.Equal("Last line!", _shellStream.ReadLine(TimeSpan.Zero));
    }

    [Fact]
    public void ReadLine_DifferentTerminators()
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello\rWorld!\nWhat's\r\ngoing\n\ron?\n"));

        Assert.Equal("Hello", _shellStream.ReadLine());
        Assert.Equal("World!", _shellStream.ReadLine());
        Assert.Equal("What's", _shellStream.ReadLine());
        Assert.Equal("going", _shellStream.ReadLine());
        Assert.Equal("", _shellStream.ReadLine());
        Assert.Equal("on?", _shellStream.ReadLine());
        Assert.Null(_shellStream.ReadLine(TimeSpan.Zero));
    }

    [Theory]
    [InlineData("\r\n")]
    [InlineData("\r")]
    [InlineData("\n")]
    public void Read_MultipleLines(string newLine)
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes(newLine));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Second line!" + newLine + "Third line!" + newLine));

        Assert.Equal("Hello World!" + newLine + "Second line!" + newLine + "Third line!" + newLine, _shellStream.Read());
    }

    [Fact]
    public async Task Read_NonEmptyArray_OnlyReturnsZeroAfterClose()
    {
        Task<int> readTask = _shellStream.ReadAsync(new byte[16], 0, 16, TestContext.Current.CancellationToken);

        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.False(readTask.IsCompleted);

        _channelSessionStub.Close();

        Assert.Equal(0, await readTask);
    }

    [Fact]
    public async Task Read_EmptyArray_OnlyReturnsZeroWhenDataAvailable()
    {
        Task<int> readTask = _shellStream.ReadAsync(Array.Empty<byte>(), 0, 0, TestContext.Current.CancellationToken);

        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.False(readTask.IsCompleted);

        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello World!"));

        Assert.Equal(0, await readTask);
    }

    [Fact]
    public async Task Read_EmptySpan_OnlyReturnsZeroWhenDataAvailable()
    {
        ValueTask<int> readTask = _shellStream.ReadAsync(Memory<byte>.Empty, TestContext.Current.CancellationToken);

        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.False(readTask.IsCompleted);

        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello World!"));

        Assert.Equal(0, await readTask);
    }

    [Fact]
    public void Expect()
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));

        Assert.Null(_shellStream.Expect("123", TimeSpan.Zero));

        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("\r\n12345"));

        Assert.Equal("Hello World!\r\n123", _shellStream.Expect("123"));
        Assert.Equal("45", _shellStream.Read());
    }

    [Fact]
    public void Read_AfterDispose_StillWorks()
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello World!"));

        _shellStream.Dispose();
#pragma warning disable S3966 // Objects should not be disposed more than once
        _shellStream.Dispose(); // Check that multiple Dispose is OK.
#pragma warning restore S3966 // Objects should not be disposed more than once

        Assert.True(_shellStream.CanRead);
        Assert.Equal("Hello World!", _shellStream.ReadLine());
        Assert.Null(_shellStream.ReadLine());
    }

    [Fact]
    public async Task ReadAsyncDoesNotBlockWriteAsync()
    {
        byte[] buffer = new byte[16];
        Task<int> readTask = _shellStream.ReadAsync(buffer, 0, buffer.Length, TestContext.Current.CancellationToken);

        await _shellStream.WriteAsync("ls\n"u8.ToArray(), 0, 3, TestContext.Current.CancellationToken);

        Assert.False(readTask.IsCompleted);

        _channelSessionStub.Receive("Directory.Build.props"u8.ToArray());

        await readTask;
    }

    [Fact]
    public void Read_MultiByte()
    {
        _channelSessionStub.Receive(new byte[] { 0xF0 });
        _channelSessionStub.Receive(new byte[] { 0x9F });
        _channelSessionStub.Receive(new byte[] { 0x91 });
        _channelSessionStub.Receive(new byte[] { 0x8D });

        Assert.Equal("👍", _shellStream.Read());
    }

    [Fact]
    public void ReadLine_MultiByte()
    {
        _channelSessionStub.Receive(new byte[] { 0xF0 });
        _channelSessionStub.Receive(new byte[] { 0x9F });
        _channelSessionStub.Receive(new byte[] { 0x91 });
        _channelSessionStub.Receive(new byte[] { 0x8D });
        _channelSessionStub.Receive(new byte[] { 0x0D });
        _channelSessionStub.Receive(new byte[] { 0x0A });

        Assert.Equal("👍", _shellStream.ReadLine());
        Assert.Equal("", _shellStream.Read());
    }

    [Fact]
    public void Expect_Regex_MultiByte()
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("𐓏𐓘𐓻𐓘𐓻𐓟 𐒻𐓟"));

        Assert.Equal("𐓏𐓘𐓻𐓘𐓻𐓟 ", _shellStream.Expect(new Regex(@"\s")));
        Assert.Equal("𐒻𐓟", _shellStream.Read());
    }

    [Fact]
    public void Expect_String_MultiByte()
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("hello 你好"));

        Assert.Equal("hello 你好", _shellStream.Expect("你好"));
        Assert.Equal("", _shellStream.Read());
    }

    [Fact]
    public void Expect_Regex_non_ASCII_characters()
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello, こんにちは, Bonjour"));

        Assert.Equal("Hello, こ", _shellStream.Expect(new Regex(@"[^\u0000-\u007F]")));

        Assert.Equal("んにちは, Bonjour", _shellStream.Read());
    }

    [Fact]
    public void Expect_String_LargeExpect()
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('a', 100)));
        for (var i = 0; i < 10; i++)
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('b', 100)));
        }
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello, こんにちは, Bonjour"));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('c', 100)));

        Assert.Equal($"{new string('a', 100)}{new string('b', 1000)}Hello, こんにちは, Bonjour", _shellStream.Expect($"{new string('b', 1000)}Hello, こんにちは, Bonjour"));

        Assert.Equal($"{new string('c', 100)}", _shellStream.Read());
    }

    [Fact]
    public void Expect_String_WithLookback()
    {
        const string expected = "ccccc";

        // Prime buffer
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string(' ', BufferSize)));

        // Test data
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('a', 100)));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('b', 100)));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes(expected));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('d', 100)));
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('e', 100)));

        // Expected result
        var expectedResult = $"{new string(' ', BufferSize)}{new string('a', 100)}{new string('b', 100)}{expected}";
        var expectedRead = $"{new string('d', 100)}{new string('e', 100)}";

        Assert.Equal(expectedResult, _shellStream.Expect(expected, TimeSpan.Zero, lookback: 250));

        Assert.Equal(expectedRead, _shellStream.Read());
    }

    [Fact]
    public void Expect_Regex_WithLookback()
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("0123456789"));

        Assert.Equal("01234567", _shellStream.Expect(new Regex(@"\d"), TimeSpan.Zero, lookback: 3));

        Assert.Equal("89", _shellStream.Read());
    }

    [Fact]
    public void Expect_Regex_WithLookback_non_ASCII_characters()
    {
        _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello, こんにちは, Bonjour"));

        Assert.Equal("Hello, こんにち", _shellStream.Expect(new Regex(@"[^\u0000-\u007F]"), TimeSpan.Zero, lookback: 11));

        Assert.Equal("は, Bonjour", _shellStream.Read());
    }

    [Fact]
    public void Expect_Timeout()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Assert.Null(_shellStream.Expect("Hello World!", TimeSpan.FromMilliseconds(200)));

        TimeSpan elapsed = stopwatch.Elapsed;

        // Account for variance in system timer resolution.
        Assert.True(elapsed > TimeSpan.FromMilliseconds(180), elapsed.ToString());
    }

    private class ChannelSessionStub : IChannelSession
    {
        public void Receive(byte[] data)
        {
            DataReceived.Invoke(this, new ChannelDataEventArgs(channelNumber: 0, data));
        }

        public void Close()
        {
            Closed.Invoke(this, new ChannelEventArgs(channelNumber: 0));
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
        public event EventHandler<ChannelEventArgs> Closed;
#pragma warning disable 0067
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
