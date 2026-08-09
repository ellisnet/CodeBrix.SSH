using System;
using CodeBrix.SSH.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Common; //was previously: Renci.SshNet.Tests.Classes.Common;

public class ExtensionsTest_Take_OffsetAndCount
{
    private Random _random;

    public ExtensionsTest_Take_OffsetAndCount()
    {
        Init();
    }

    private void Init()
    {
        _random = new Random();
    }

    [Fact]
    public void ShouldThrowArgumentNullExceptionWhenValueIsNull()
    {
        const byte[] value = null;
        const int offset = 0;
        const int count = 0;

        try
        {
            Extensions.Take(value, offset, count);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("value", ex.ParamName);
        }
    }

    [Fact]
    public void ShouldReturnEmptyByteArrayWhenCountIsZero()
    {
        var value = CreateBuffer(16);
        const int offset = 25;
        const int count = 0;

        var actual = Extensions.Take(value, offset, count);

        Assert.NotNull(actual);
        Assert.Empty(actual);
    }

    [Fact]
    public void ShouldReturnValueWhenCountIsEqualToLengthOfValueAndOffsetIsZero()
    {
        var value = CreateBuffer(16);
        const int offset = 0;
        var count = value.Length;

        var actual = Extensions.Take(value, offset, count);

        Assert.NotNull(actual);
        Assert.Equal(value.Length, actual.Length);
        Assert.Equal(value, actual);
    }

    [Fact]
    public void ShouldReturnLeadingBytesWhenOffsetIsZeroAndCountIsLessThanLengthOfValue()
    {
        var value = CreateBuffer(16);
        const int offset = 0;
        const int count = 5;

        var actual = Extensions.Take(value, offset, count);

        Assert.NotNull(actual);
        Assert.Equal(count, actual.Length);
        Assert.Equal(value[0], actual[0]);
        Assert.Equal(value[1], actual[1]);
        Assert.Equal(value[2], actual[2]);
        Assert.Equal(value[3], actual[3]);
        Assert.Equal(value[4], actual[4]);
    }

    [Fact]
    public void ShouldReturnCorrectPartOfValueWhenOffsetIsGreaterThanZeroAndOffsetPlusCountIsLessThanLengthOfValue()
    {
        var value = CreateBuffer(16);
        const int offset = 3;
        const int count = 4;

        var actual = Extensions.Take(value, offset, count);

        Assert.NotNull(actual);
        Assert.Equal(count, actual.Length);
        Assert.Equal(value[3], actual[0]);
        Assert.Equal(value[4], actual[1]);
        Assert.Equal(value[5], actual[2]);
        Assert.Equal(value[6], actual[3]);
    }

    [Fact]
    public void ShouldThrowArgumentExceptionWhenCountIsGreaterThanLengthOfValue()
    {
        var value = CreateBuffer(16);
        const int offset = 0;
        var count = value.Length + 1;

        try
        {
            Extensions.Take(value, offset, count);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentException)
        {
        }
    }

    [Fact]
    public void ShouldThrowArgumentExceptionWhenOffsetPlusCountIsGreaterThanLengthOfValue()
    {
        var value = CreateBuffer(16);
        const int offset = 1;
        var count = value.Length;

        try
        {
            Extensions.Take(value, offset, count);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentException)
        {
        }
    }

    private byte[] CreateBuffer(int length)
    {
        var buffer = new byte[length];
        _random.NextBytes(buffer);
        return buffer;
    }
}
