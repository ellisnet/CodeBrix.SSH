using System;
using CodeBrix.SSH.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Common; //was previously: Renci.SshNet.Tests.Classes.Common;

public class ExtensionsTest_IsEqualTo_ByteArray
{
    private Random _random;

    public ExtensionsTest_IsEqualTo_ByteArray()
    {
        Init();
    }

    private void Init()
    {
        _random = new Random();
    }

    [Fact]
    public void ShouldThrowArgumentNullExceptionWhenLeftIsNull()
    {
        const byte[] left = null;
        var right = CreateBuffer(1);

        try
        {
            _ = Extensions.IsEqualTo(left, right);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("left", ex.ParamName);
        }
    }

    [Fact]
    public void ShouldThrowArgumentNullExceptionWhenRightIsNull()
    {
        var left = CreateBuffer(1);
        const byte[] right = null;

        try
        {
            _ = Extensions.IsEqualTo(left, right);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("right", ex.ParamName);
        }
    }

    [Fact]
    public void ShouldThrowArgumentNullExceptionWhenLeftAndRightAreNull()
    {
        const byte[] left = null;
        const byte[] right = null;

        try
        {
            _ = Extensions.IsEqualTo(left, right);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("left", ex.ParamName);
        }
    }

    [Fact]
    public void ShouldReturnFalseWhenLeftIsNotEqualToRight()
    {
        Assert.False(Extensions.IsEqualTo(new byte[] { 0x0a }, new byte[] { 0x0a, 0x0d }));
        Assert.False(Extensions.IsEqualTo(new byte[] { 0x0a, 0x0d }, new byte[] { 0x0a }));
        Assert.False(Extensions.IsEqualTo(new byte[0], new byte[] { 0x0a }));
        Assert.False(Extensions.IsEqualTo(new byte[] { 0x0a, 0x0d }, new byte[0]));
    }

    [Fact]
    public void ShouldReturnTrueWhenLeftIsEqualToRight()
    {
        Assert.True(Extensions.IsEqualTo(new byte[] { 0x0a, 0x0d }, new byte[] { 0x0a, 0x0d }));
        Assert.True(Extensions.IsEqualTo(new byte[0], new byte[0]));
    }

    [Fact]
    public void ShouldReturnTrueWhenLeftIsSameAsRight()
    {
        var left = new byte[] { 0x0d, 0x0d };

        Assert.True(Extensions.IsEqualTo(left, left));
    }

    private byte[] CreateBuffer(int length)
    {
        var buffer = new byte[length];
        _random.NextBytes(buffer);
        return buffer;
    }
}
