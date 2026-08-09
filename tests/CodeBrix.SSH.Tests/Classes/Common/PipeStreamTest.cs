using System;
using System.IO;
using System.Threading.Tasks;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Common; //was previously: Renci.SshNet.Tests.Classes.Common;

public class PipeStreamTest : TestBase
{
    [Fact]
    public void Test_PipeStream_Write_Read_Buffer()
    {
        var testBuffer = new byte[1024];
        new Random().NextBytes(testBuffer);

        var outputBuffer = new byte[1024];

        using (var stream = new PipeStream())
        {
            stream.Write(testBuffer, 0, 512);

            Assert.Equal(512, stream.Length);

            Assert.Equal(128, stream.Read(outputBuffer, 64, 128));

            Assert.Equal(384, stream.Length);

            Assert.Equal(new byte[64].Concat(testBuffer.Take(128)).Concat(new byte[832]), outputBuffer);
        }
    }

    [Fact]
    public void Test_PipeStream_Write_Read_Byte()
    {
        var testBuffer = new byte[1024];
        new Random().NextBytes(testBuffer);

        using (var stream = new PipeStream())
        {
            stream.Write(testBuffer, 0, testBuffer.Length);
            Assert.Equal(1024, stream.Length);
            Assert.Equal(testBuffer[0], stream.ReadByte());
            Assert.Equal(1023, stream.Length);
            Assert.Equal(testBuffer[1], stream.ReadByte());
            Assert.Equal(1022, stream.Length);
        }
    }

    [Fact]
    public void Read()
    {
        var target = new PipeStream();
        target.WriteByte(0x0a);
        target.WriteByte(0x0d);
        target.WriteByte(0x09);

        var readBuffer = new byte[2];
        var bytesRead = target.Read(readBuffer, 0, readBuffer.Length);
        Assert.Equal(2, bytesRead);
        Assert.Equal(0x0a, readBuffer[0]);
        Assert.Equal(0x0d, readBuffer[1]);

        var writeBuffer = new byte[] { 0x05, 0x03 };
        target.Write(writeBuffer, 0, writeBuffer.Length);

        readBuffer = new byte[4];
        bytesRead = target.Read(readBuffer, 0, readBuffer.Length);
        Assert.Equal(3, bytesRead);
        Assert.Equal(0x09, readBuffer[0]);
        Assert.Equal(0x05, readBuffer[1]);
        Assert.Equal(0x03, readBuffer[2]);
        Assert.Equal(0x00, readBuffer[3]);
    }

    [Fact]
    public async Task Read_NonEmptyArray_OnlyReturnsZeroAfterDispose()
    {
        // When there is no data available, a read should block,
        // but then unblock (and return 0) after disposal.

        var pipeStream = new PipeStream();

        Task<int> readTask = pipeStream.ReadAsync(new byte[16], 0, 16, TestContext.Current.CancellationToken);

        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.False(readTask.IsCompleted);

        await pipeStream.DisposeAsync();

        Assert.Equal(0, await readTask);
    }

    [Fact]
    public async Task Read_EmptyArray_OnlyReturnsZeroAfterDispose()
    {
        // Similarly, zero byte reads should still block until after disposal.

        var pipeStream = new PipeStream();

        Task<int> readTask = pipeStream.ReadAsync(Array.Empty<byte>(), 0, 0, TestContext.Current.CancellationToken);

        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.False(readTask.IsCompleted);

        await pipeStream.DisposeAsync();

        Assert.Equal(0, await readTask);
    }

    [Fact]
    public async Task Read_EmptyArray_OnlyReturnsZeroWhenDataAvailable()
    {
        // And zero byte reads should block but then return 0 once data
        // is available.

        var pipeStream = new PipeStream();

        Task<int> readTask = pipeStream.ReadAsync(Array.Empty<byte>(), 0, 0, TestContext.Current.CancellationToken);

        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.False(readTask.IsCompleted);

        await pipeStream.WriteAsync(new byte[] { 1, 2, 3, 4 }, 0, 4, TestContext.Current.CancellationToken);

        Assert.Equal(0, await readTask);
    }

    [Fact]
    public async Task Read_EmptySpan_OnlyReturnsZeroWhenDataAvailable()
    {
        // And zero byte reads should block but then return 0 once data
        // is available (the span version).

        var pipeStream = new PipeStream();

        ValueTask<int> readTask = pipeStream.ReadAsync(Memory<byte>.Empty, TestContext.Current.CancellationToken);

        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.False(readTask.IsCompleted);

        await pipeStream.WriteAsync(new byte[] { 1, 2, 3, 4 }, TestContext.Current.CancellationToken);

        Assert.Equal(0, await readTask);
    }

    [Fact]
    public void Read_AfterDispose_StillWorks()
    {
        var pipeStream = new PipeStream();

        pipeStream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);

        pipeStream.Dispose();
#pragma warning disable S3966 // Objects should not be disposed more than once
        pipeStream.Dispose(); // Check that multiple Dispose is OK.
#pragma warning restore S3966 // Objects should not be disposed more than once

        Assert.True(pipeStream.CanRead);

        Assert.Equal(4, pipeStream.Read(new byte[5], 0, 5));
        Assert.Equal(0, pipeStream.Read(new byte[5], 0, 5));
    }

    [Fact]
    public void SeekShouldThrowNotSupportedException()
    {
        var target = new PipeStream();
        Assert.ThrowsAny<NotSupportedException>(() => target.Seek(offset: 0, SeekOrigin.Begin));
    }

    [Fact]
    public void SetLengthShouldThrowNotSupportedException()
    {
        var target = new PipeStream();
        Assert.ThrowsAny<NotSupportedException>(() => target.SetLength(1));
    }

    [Fact]
    public void WriteTest()
    {
        var target = new PipeStream();

        var writeBuffer = new byte[] { 0x0a, 0x05, 0x0d };
        target.Write(writeBuffer, 0, 2);

        writeBuffer = new byte[] { 0x02, 0x04, 0x03, 0x06, 0x09 };
        target.Write(writeBuffer, 1, 2);

        var readBuffer = new byte[6];
        var bytesRead = target.Read(readBuffer, 0, 4);

        Assert.Equal(4, bytesRead);
        Assert.Equal(0x0a, readBuffer[0]);
        Assert.Equal(0x05, readBuffer[1]);
        Assert.Equal(0x04, readBuffer[2]);
        Assert.Equal(0x03, readBuffer[3]);
        Assert.Equal(0x00, readBuffer[4]);
        Assert.Equal(0x00, readBuffer[5]);
    }

    [Fact]
    public void WriteTest_Span()
    {
        var target = new PipeStream();

        var writeBuffer = new byte[] { 0x0a, 0x05, 0x0d };
        target.Write(writeBuffer.AsSpan(0, 2));

        writeBuffer = new byte[] { 0x02, 0x04, 0x03, 0x06, 0x09 };
        target.Write(writeBuffer.AsSpan(1, 2));

        var readBuffer = new byte[6];
        var bytesRead = target.Read(readBuffer.AsSpan(0, 4));

        Assert.Equal(4, bytesRead);
        Assert.Equal(0x0a, readBuffer[0]);
        Assert.Equal(0x05, readBuffer[1]);
        Assert.Equal(0x04, readBuffer[2]);
        Assert.Equal(0x03, readBuffer[3]);
        Assert.Equal(0x00, readBuffer[4]);
        Assert.Equal(0x00, readBuffer[5]);
    }

    [Fact]
    public void CanReadTest()
    {
        var target = new PipeStream();
        Assert.True(target.CanRead);
    }

    [Fact]
    public void CanSeekTest()
    {
        var target = new PipeStream();
        Assert.False(target.CanSeek);
    }

    [Fact]
    public void CanWriteTest()
    {
        var target = new PipeStream();
        Assert.True(target.CanWrite);
        target.Dispose();
        Assert.False(target.CanWrite);
    }

    [Fact]
    public void LengthTest()
    {
        var target = new PipeStream();
        Assert.Equal(0L, target.Length);
        target.Write(new byte[] { 0x0a, 0x05, 0x0d }, 0, 2);
        Assert.Equal(2L, target.Length);
        target.WriteByte(0x0a);
        Assert.Equal(3L, target.Length);
        _ = target.Read(new byte[2], 0, 2);
        Assert.Equal(1L, target.Length);
        _ = target.ReadByte();
        Assert.Equal(0L, target.Length);
    }

    [Fact]
    public void Position_GetterAlwaysReturnsZero()
    {
        var target = new PipeStream();

        Assert.Equal(0, target.Position);
        target.WriteByte(0x0a);
        Assert.Equal(0, target.Position);
        _ = target.ReadByte();
        Assert.Equal(0, target.Position);
    }

    [Fact]
    public void Position_SetterAlwaysThrowsNotSupportedException()
    {
        var target = new PipeStream();
        Assert.ThrowsAny<NotSupportedException>(() => target.Position = 0);
    }
}
