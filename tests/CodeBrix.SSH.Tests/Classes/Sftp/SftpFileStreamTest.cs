using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Responses;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp; //was previously: Renci.SshNet.Tests.Classes.Sftp;

public class SftpFileStreamTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BadFileMode_ThrowsArgumentOutOfRangeException(bool isAsync)
    {
        ArgumentOutOfRangeException ex;

        if (isAsync)
        {
            ex = await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(() =>
                SftpFileStream.OpenAsync(new Mock<ISftpSession>().Object, "file.txt", mode: 0, FileAccess.Read, bufferSize: 1024, CancellationToken.None));
        }
        else
        {
            ex = Assert.ThrowsAny<ArgumentOutOfRangeException>(() =>
                SftpFileStream.Open(new Mock<ISftpSession>().Object, "file.txt", mode: 0, FileAccess.Read, bufferSize: 1024));
        }

        Assert.Equal("mode", ex.ParamName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BadFileAccess_ThrowsArgumentOutOfRangeException(bool isAsync)
    {
        ArgumentOutOfRangeException ex;

        if (isAsync)
        {
            ex = await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(() =>
                SftpFileStream.OpenAsync(new Mock<ISftpSession>().Object, "file.txt", FileMode.Open, access: 0, bufferSize: 1024, CancellationToken.None));
        }
        else
        {
            ex = Assert.ThrowsAny<ArgumentOutOfRangeException>(() =>
                SftpFileStream.Open(new Mock<ISftpSession>().Object, "file.txt", FileMode.Open, access: 0, bufferSize: 1024));
        }

        Assert.Equal("access", ex.ParamName);
    }

    [Theory]
    [InlineData(FileMode.Append, FileAccess.Read, false)]
    [InlineData(FileMode.Append, FileAccess.Read, true)]
    [InlineData(FileMode.Append, FileAccess.ReadWrite, false)]
    [InlineData(FileMode.Append, FileAccess.ReadWrite, true)]
    [InlineData(FileMode.Create, FileAccess.Read, false)]
    [InlineData(FileMode.Create, FileAccess.Read, true)]
    [InlineData(FileMode.CreateNew, FileAccess.Read, false)]
    [InlineData(FileMode.CreateNew, FileAccess.Read, true)]
    [InlineData(FileMode.Truncate, FileAccess.Read, false)]
    [InlineData(FileMode.Truncate, FileAccess.Read, true)]
    public async Task InvalidModeAccessCombination_ThrowsArgumentException(FileMode mode, FileAccess access, bool isAsync)
    {
        ArgumentException ex;

        if (isAsync)
        {
            ex = await Assert.ThrowsAnyAsync<ArgumentException>(() =>
                SftpFileStream.OpenAsync(new Mock<ISftpSession>().Object, "file.txt", mode, access, bufferSize: 1024, CancellationToken.None));
        }
        else
        {
            ex = Assert.ThrowsAny<ArgumentException>(() =>
                SftpFileStream.Open(new Mock<ISftpSession>().Object, "file.txt", mode, access, bufferSize: 1024));
        }

        Assert.Equal("access", ex.ParamName);
    }

    [Fact]
    public void ReadWithWriteAccess_ThrowsNotSupportedException()
    {
        var sessionMock = new Mock<ISftpSession>();

        sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
        sessionMock.Setup(s => s.IsOpen).Returns(true);

        SetupRemoteSize(sessionMock, 128);

        var s = SftpFileStream.Open(sessionMock.Object, "file.txt", FileMode.Create, FileAccess.Write, bufferSize: 1024);

        Assert.False(s.CanRead);

        Assert.ThrowsAny<NotSupportedException>(() => _ = s.Read(new byte[4], 0, 4));
        Assert.ThrowsAny<NotSupportedException>(() => _ = s.ReadByte());
        Assert.ThrowsAny<NotSupportedException>(() => _ = s.ReadAsync(new byte[4], 0, 4, TestContext.Current.CancellationToken).GetAwaiter().GetResult());
        Assert.ThrowsAny<NotSupportedException>(() => _ = s.EndRead(s.BeginRead(new byte[4], 0, 4, null, null)));
        Assert.ThrowsAny<NotSupportedException>(() => _ = s.Read(new byte[4]));
        Assert.ThrowsAny<NotSupportedException>(() => _ = s.ReadAsync(new byte[4], TestContext.Current.CancellationToken).AsTask().GetAwaiter().GetResult());
        Assert.ThrowsAny<NotSupportedException>(() => s.CopyTo(Stream.Null));
        Assert.ThrowsAny<NotSupportedException>(() => s.CopyToAsync(Stream.Null, TestContext.Current.CancellationToken).GetAwaiter().GetResult());
    }

    [Fact]
    public void WriteWithReadAccess_ThrowsNotSupportedException()
    {
        var sessionMock = new Mock<ISftpSession>();

        sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
        sessionMock.Setup(s => s.IsOpen).Returns(true);

        var s = SftpFileStream.Open(sessionMock.Object, "file.txt", FileMode.Open, FileAccess.Read, bufferSize: 1024);

        Assert.False(s.CanWrite);

        Assert.ThrowsAny<NotSupportedException>(() => s.Write(new byte[4], 0, 4));
        Assert.ThrowsAny<NotSupportedException>(() => s.WriteByte(0xf));
        Assert.ThrowsAny<NotSupportedException>(() => s.WriteAsync(new byte[4], 0, 4, TestContext.Current.CancellationToken).GetAwaiter().GetResult());
        Assert.ThrowsAny<NotSupportedException>(() => s.EndWrite(s.BeginWrite(new byte[4], 0, 4, null, null)));
        Assert.ThrowsAny<NotSupportedException>(() => s.Write(new byte[4]));
        Assert.ThrowsAny<NotSupportedException>(() => s.WriteAsync(new byte[4], TestContext.Current.CancellationToken).AsTask().GetAwaiter().GetResult());
        Assert.ThrowsAny<NotSupportedException>(() => s.SetLength(1024));
    }

    [Theory]
    [InlineData(-1, SeekOrigin.Begin)]
    [InlineData(-1, SeekOrigin.Current)]
    [InlineData(-1000, SeekOrigin.End)]
    public void SeekBeforeBeginning_ThrowsIOException(long offset, SeekOrigin origin)
    {
        var sessionMock = new Mock<ISftpSession>();

        sessionMock.Setup(s => s.CalculateOptimalReadLength(It.IsAny<uint>())).Returns<uint>(x => x);
        sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
        sessionMock.Setup(s => s.IsOpen).Returns(true);

        SetupRemoteSize(sessionMock, 128);

        var s = SftpFileStream.Open(sessionMock.Object, "file.txt", FileMode.Open, FileAccess.Read, bufferSize: 1024);

        Assert.ThrowsAny<IOException>(() => s.Seek(offset, origin));
    }

    private static void SetupRemoteSize(Mock<ISftpSession> sessionMock, long size)
    {
        sessionMock.Setup(s => s.RequestFStat(It.IsAny<byte[]>())).Returns(new SftpFileAttributes(
            default, default, size: size, default, default, default, default
            ));
    }

    // Operations which should cause writes to be flushed because they depend on
    // the remote file being up to date.
    // Most of these are already implicitly covered by integration tests and may
    // not be so valuable here.
    [Fact]
    public void Flush_SendsBufferedWrites()
    {
        TestSendsBufferedWrites(s => s.Flush());
    }

    [Fact]
    public void Read_SendsBufferedWrites()
    {
        TestSendsBufferedWrites(s => _ = s.Read(new byte[16], 0, 16));
    }

    [Fact]
    public void Seek_SendsBufferedWrites()
    {
        TestSendsBufferedWrites(s => _ = s.Seek(-1, SeekOrigin.Current));
    }

    [Fact]
    public void SetPosition_SendsBufferedWrites()
    {
        TestSendsBufferedWrites(s => s.Position++);
    }

    [Fact]
    public void SetLength_SendsBufferedWrites()
    {
        TestSendsBufferedWrites(s => s.SetLength(256));
    }

    [Fact]
    public void GetLength_SendsBufferedWrites()
    {
        TestSendsBufferedWrites(s => _ = s.Length);
    }

    [Fact]
    public void Dispose_SendsBufferedWrites()
    {
        TestSendsBufferedWrites(s => s.Dispose());
    }

    private void TestSendsBufferedWrites(Action<SftpFileStream> flushAction)
    {
        var sessionMock = new Mock<ISftpSession>();

        sessionMock.Setup(s => s.CalculateOptimalReadLength(It.IsAny<uint>())).Returns<uint>(x => x);
        sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
        sessionMock.Setup(s => s.IsOpen).Returns(true);
        SetupRemoteSize(sessionMock, 0);

        var s = SftpFileStream.Open(sessionMock.Object, "file.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, bufferSize: 1024);

        // Buffer some data
        byte[] newData = "Some new bytes"u8.ToArray();
        s.Write(newData, 0, newData.Length);

        byte[] newData2 = "Some more bytes"u8.ToArray();
        s.Write(newData2, 0, newData2.Length);

        // The written data does not exceed bufferSize so we do not expect
        // it to have been sent.
        sessionMock.Verify(s => s.RequestWrite(
            It.IsAny<byte[]>(),
            It.IsAny<ulong>(),
            It.IsAny<byte[]>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<AutoResetEvent>(),
            It.IsAny<Action<SftpStatusResponse>>()),
            Times.Never);

        // Whatever is called here should trigger the bytes to be sent
        flushAction(s);

        VerifyRequestWrite(sessionMock, newData.Concat(newData2), serverOffset: 0);
    }

    [Fact]
    public void Dispose()
    {
        var sessionMock = new Mock<ISftpSession>();

        sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
        sessionMock.Setup(s => s.IsOpen).Returns(true);

        var s = SftpFileStream.Open(sessionMock.Object, "file.txt", FileMode.Create, FileAccess.ReadWrite, bufferSize: 1024);

        Assert.True(s.CanRead);
        Assert.True(s.CanWrite);

        s.Dispose();
        sessionMock.Verify(p => p.RequestClose(It.IsAny<byte[]>()), Times.Once);

        Assert.False(s.CanRead);
        Assert.False(s.CanSeek);
        Assert.False(s.CanWrite);

        Assert.ThrowsAny<ObjectDisposedException>(() => s.Read(new byte[16], 0, 16));
        Assert.ThrowsAny<ObjectDisposedException>(() => s.ReadByte());
        Assert.ThrowsAny<ObjectDisposedException>(() => s.Write(new byte[16], 0, 16));
        Assert.ThrowsAny<ObjectDisposedException>(() => s.WriteByte(0xf));
        Assert.ThrowsAny<ObjectDisposedException>(() => s.CopyTo(Stream.Null));
        Assert.ThrowsAny<ObjectDisposedException>(s.Flush);
        Assert.ThrowsAny<ObjectDisposedException>(() => s.Seek(0, SeekOrigin.Begin));
        Assert.ThrowsAny<ObjectDisposedException>(() => s.SetLength(128));
        Assert.ThrowsAny<ObjectDisposedException>(() => _ = s.Length);

        // Test no-op second dispose
        s.Dispose();
        sessionMock.Verify(p => p.RequestClose(It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public void FstatFailure_DisablesSeek()
    {
        TestFstatFailure(fstat => fstat.Throws<SftpPermissionDeniedException>());
    }

    [Fact]
    public void FstatSizeNotReturned_DisablesSeek()
    {
        TestFstatFailure(fstat => fstat.Returns(SftpFileAttributes.FromBytes([0, 0, 0, 0])));
    }

    private void TestFstatFailure(Action<CodeBrix.TestMocks.Mocking.Language.Flow.ISetup<ISftpSession, SftpFileAttributes>> fstatSetup)
    {
        var sessionMock = new Mock<ISftpSession>();

        sessionMock.Setup(s => s.CalculateOptimalReadLength(It.IsAny<uint>())).Returns<uint>(x => x);
        sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
        sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        sessionMock.Setup(s => s.IsOpen).Returns(true);

        fstatSetup(sessionMock.Setup(s => s.RequestFStat(It.IsAny<byte[]>())));

        var s = SftpFileStream.Open(sessionMock.Object, "file.txt", FileMode.Open, FileAccess.ReadWrite, bufferSize: 1024);

        Assert.False(s.CanSeek);
        Assert.True(s.CanRead);
        Assert.True(s.CanWrite);

        Assert.ThrowsAny<NotSupportedException>(() => s.Position);
        Assert.ThrowsAny<NotSupportedException>(() => s.Length);
        Assert.ThrowsAny<NotSupportedException>(() => s.Seek(0, SeekOrigin.Begin));
        Assert.ThrowsAny<NotSupportedException>(() => s.SetLength(1024));

        // Reads and writes still succeed.
        _ = s.Read(new byte[16], 0, 16);
        s.Write(new byte[16], 0, 16);
        s.Flush();
    }

    private static void VerifyRequestWrite(Mock<ISftpSession> sessionMock, ReadOnlyMemory<byte> newData, int serverOffset)
    {
        sessionMock.Verify(s => s.RequestWrite(
            /* handle: */         It.IsAny<byte[]>(),
            /* serverOffset: */   (ulong)serverOffset,
            /* data: */           It.Is<byte[]>(x => IndexOf(x, newData) >= 0),
            /* offset: */         It.IsAny<int>(),
            /* length: */         newData.Length,
            /* wait: */           It.IsAny<AutoResetEvent>(),
            /* writeCompleted: */ It.IsAny<Action<SftpStatusResponse>>()),
            Times.Once);
    }

    private static int IndexOf(byte[] searchSpace, ReadOnlyMemory<byte> searchValue)
    {
        // Needed in a (non-local) function because expression lambdas can't contain spans
        return searchSpace.AsSpan().IndexOf(searchValue.Span);
    }
}
