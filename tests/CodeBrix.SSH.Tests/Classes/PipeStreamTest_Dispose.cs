using System;
using System.IO;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class PipeStreamTest_Dispose : TestBase
{
    private PipeStream _pipeStream;

    protected override void OnInit()
    {
        base.OnInit();

        Arrange();
        Act();
    }

    private void Arrange()
    {
        _pipeStream = new PipeStream();
    }

    private void Act()
    {
        _pipeStream.Dispose();
    }

    [Fact]
    public void CanRead_ShouldReturnTrue()
    {
        Assert.True(_pipeStream.CanRead);
    }

    [Fact]
    public void Flush_ShouldNotThrow()
    {
        _pipeStream.Flush();
    }

    [Fact]
    public void Length_ShouldNotThrow()
    {
        _ = _pipeStream.Length;
    }

    [Fact]
    public void Position_Getter_ShouldReturnZero()
    {
        Assert.Equal(0, _pipeStream.Position);
    }

    [Fact]
    public void Position_Setter_ShouldThrowNotSupportedException()
    {
        try
        {
            _pipeStream.Position = 0;
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (NotSupportedException)
        {
        }
    }

    [Fact]
    public void Read_ByteArrayAndOffsetAndCount_ShouldNotThrow()
    {
        Assert.Equal(0, _pipeStream.Read(new byte[1], 0, 1));
    }

    [Fact]
    public void ReadByte_ShouldNotThrow()
    {
        Assert.Equal(-1, _pipeStream.ReadByte());
    }

    [Fact]
    public void Seek_ShouldThrowNotSupportedException()
    {
        try
        {
            _pipeStream.Seek(0, SeekOrigin.Begin);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (NotSupportedException)
        {
        }
    }

    [Fact]
    public void SetLength_ShouldThrowNotSupportedException()
    {
        try
        {
            _pipeStream.SetLength(0);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (NotSupportedException)
        {
        }
    }

    [Fact]
    public void Write_ByteArrayAndOffsetAndCount_ShouldThrowObjectDisposedException()
    {
        var buffer = new byte[0];
        const int offset = 0;
        const int count = 0;

        try
        {
            _pipeStream.Write(buffer, offset, count);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ObjectDisposedException)
        {
        }
    }

    [Fact]
    public void WriteByte_ShouldThrowObjectDisposedException()
    {
        const byte b = 0x0a;

        try
        {
            _pipeStream.WriteByte(b);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ObjectDisposedException)
        {
        }
    }
}
