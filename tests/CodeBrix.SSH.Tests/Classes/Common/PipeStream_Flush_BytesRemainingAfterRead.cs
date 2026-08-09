using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Common; //was previously: Renci.SshNet.Tests.Classes.Common;

public class PipeStream_Flush_BytesRemainingAfterRead : TripleATestBase
{
    private PipeStream _pipeStream;
    private byte[] _readBuffer;
    private int _bytesRead;
    private Thread _readThread;

    protected override void Arrange()
    {
        _pipeStream = new PipeStream();
        _pipeStream.WriteByte(10);
        _pipeStream.WriteByte(13);
        _pipeStream.WriteByte(15);
        _pipeStream.WriteByte(18);
        _pipeStream.WriteByte(23);
        _pipeStream.WriteByte(28);

        _bytesRead = 0;
        _readBuffer = new byte[4];

        _readThread = new Thread(() => _bytesRead = _pipeStream.Read(_readBuffer, 0, _readBuffer.Length));
        _readThread.Start();

        // ensure we've started reading
        _ = _readThread.Join(50);
    }

    protected override void Act()
    {
        _pipeStream.Flush();

        // give async read time to complete
        _ = _readThread.Join(100);
    }

    [Fact]
    public void AsyncReadShouldHaveFinished()
    {
        Assert.Equal(ThreadState.Stopped, _readThread.ThreadState);
    }

    [Fact]
    public void ReadShouldReturnNumberOfBytesAvailableThatAreWrittenToBuffer()
    {
        Assert.Equal(4, _bytesRead);
    }

    [Fact]
    public void BytesAvailableInStreamShouldHaveBeenWrittenToBuffer()
    {
        Assert.Equal(10, _readBuffer[0]);
        Assert.Equal(13, _readBuffer[1]);
        Assert.Equal(15, _readBuffer[2]);
        Assert.Equal(18, _readBuffer[3]);
    }

    [Fact]
    public void RemainingBytesCanBeRead()
    {
        var buffer = new byte[3];

        var bytesRead = _pipeStream.Read(buffer, 0, 2);

        Assert.Equal(2, bytesRead);
        Assert.Equal(23, buffer[0]);
        Assert.Equal(28, buffer[1]);
        Assert.Equal(0, buffer[2]);
    }

    [Fact]
    public void ReadingMoreBytesThanAvailableDoesNotBlock()
    {
        var buffer = new byte[4];

        var bytesRead = _pipeStream.Read(buffer, 0, buffer.Length);

        Assert.Equal(2, bytesRead);
        Assert.Equal(23, buffer[0]);
        Assert.Equal(28, buffer[1]);
        Assert.Equal(0, buffer[2]);
        Assert.Equal(0, buffer[3]);
    }
}
