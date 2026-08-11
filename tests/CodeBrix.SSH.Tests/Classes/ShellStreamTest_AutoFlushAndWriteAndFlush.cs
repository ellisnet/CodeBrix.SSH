using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CodeBrix.SSH.Abstractions;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes;

public class ShellStreamTest_AutoFlushAndWriteAndFlush
{
    private const int BufferSize = 1024;

    private readonly Mock<IChannelSession> _channelSessionMock;
    private readonly ShellStream _shellStream;
    private readonly List<byte> _sentBytes = new List<byte>();

    public ShellStreamTest_AutoFlushAndWriteAndFlush()
    {
        _channelSessionMock = new Mock<IChannelSession>();
        _channelSessionMock.Setup(p => p.SendPseudoTerminalRequest(It.IsAny<string>(),
                                                                   It.IsAny<uint>(),
                                                                   It.IsAny<uint>(),
                                                                   It.IsAny<uint>(),
                                                                   It.IsAny<uint>(),
                                                                   It.IsAny<IDictionary<TerminalModes, uint>>()))
                           .Returns(true);
        _channelSessionMock.Setup(p => p.SendShellRequest())
                           .Returns(true);
        _channelSessionMock.Setup(p => p.SendData(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                           .Callback<byte[], int, int>((data, offset, count) => _sentBytes.AddRange(data.AsSpan(offset, count).ToArray()));

        var connectionInfoMock = new Mock<IConnectionInfo>();
        connectionInfoMock.Setup(p => p.Encoding).Returns(Encoding.UTF8);

        var sessionMock = new Mock<ISession>();
        sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        sessionMock.Setup(p => p.ConnectionInfo).Returns(connectionInfoMock.Object);
        sessionMock.Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);

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
    public void AutoFlush_DefaultsToFalse_AndByteWritesAreBuffered()
    {
        Assert.False(_shellStream.AutoFlush);

        _shellStream.Write(Encoding.UTF8.GetBytes("ls"), 0, 2);

        _channelSessionMock.Verify(p => p.SendData(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Write_Bytes_WithAutoFlush_IsSentImmediately()
    {
        _shellStream.AutoFlush = true;

        _shellStream.Write(Encoding.UTF8.GetBytes("ls"), 0, 2);

        Assert.Equal(Encoding.UTF8.GetBytes("ls"), _sentBytes.ToArray());
    }

    [Fact]
    public void Write_Span_WithAutoFlush_IsSentImmediately()
    {
        _shellStream.AutoFlush = true;

        _shellStream.Write(Encoding.UTF8.GetBytes("top").AsSpan());

        Assert.Equal(Encoding.UTF8.GetBytes("top"), _sentBytes.ToArray());
    }

    [Fact]
    public void WriteByte_WithAutoFlush_IsSentImmediately()
    {
        _shellStream.AutoFlush = true;

        _shellStream.WriteByte((byte)'x');

        Assert.Equal(new[] { (byte)'x' }, _sentBytes.ToArray());
    }

    [Fact]
    public async Task WriteAsync_WithAutoFlush_IsSentImmediately()
    {
        _shellStream.AutoFlush = true;

        await _shellStream.WriteAsync(Encoding.UTF8.GetBytes("pwd"), 0, 3, TestContext.Current.CancellationToken);

        Assert.Equal(Encoding.UTF8.GetBytes("pwd"), _sentBytes.ToArray());
    }

    [Fact]
    public void WriteAndFlush_Bytes_IsSentImmediatelyWithoutAutoFlush()
    {
        _shellStream.WriteAndFlush(Encoding.UTF8.GetBytes("cat"), 0, 3);

        Assert.Equal(Encoding.UTF8.GetBytes("cat"), _sentBytes.ToArray());
    }

    [Fact]
    public void WriteAndFlush_String_IsSentImmediatelyWithoutAutoFlush()
    {
        _shellStream.WriteAndFlush("echo hi");

        Assert.Equal(Encoding.UTF8.GetBytes("echo hi"), _sentBytes.ToArray());
    }

    [Fact]
    public void WriteAndFlush_NullString_FlushesPreviouslyBufferedData()
    {
        _shellStream.Write(Encoding.UTF8.GetBytes("df"), 0, 2);

        _channelSessionMock.Verify(p => p.SendData(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

        _shellStream.WriteAndFlush((string)null);

        Assert.Equal(Encoding.UTF8.GetBytes("df"), _sentBytes.ToArray());
    }
}
