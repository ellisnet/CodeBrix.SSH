using System;
using CodeBrix.SSH.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Common; //was previously: Renci.SshNet.Tests.Classes.Common;

public class ExtensionsTest_TrimLeadingZeros
{
    [Fact]
    public void ShouldThrowArgumentNullExceptionWhenValueIsNull()
    {
        const byte[] value = null;

        try
        {
            Extensions.TrimLeadingZeros(value);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("value", ex.ParamName);
        }
    }

    [Fact]
    public void ShouldRemoveAllLeadingZeros()
    {
        byte[] value = { 0x00, 0x00, 0x0a, 0x0d };

        var actual = Extensions.TrimLeadingZeros(value);

        Assert.NotNull(actual);
        Assert.Equal(2, actual.Length);
        Assert.Equal(0x0a, actual[0]);
        Assert.Equal(0x0d, actual[1]);
    }

    [Fact]
    public void ShouldOnlyRemoveLeadingZeros()
    {
        byte[] value = { 0x00, 0x0a, 0x00, 0x0d, 0x00 };

        var actual = Extensions.TrimLeadingZeros(value);

        Assert.NotNull(actual);
        Assert.Equal(4, actual.Length);
        Assert.Equal(0x0a, actual[0]);
        Assert.Equal(0x00, actual[1]);
        Assert.Equal(0x0d, actual[2]);
        Assert.Equal(0x00, actual[3]);
    }

    [Fact]
    public void ShouldReturnOriginalEmptyByteArrayWhenValueHasNoLeadingZeros()
    {
        byte[] value = { 0x0a, 0x00, 0x0d };

        var actual = Extensions.TrimLeadingZeros(value);

        Assert.NotNull(actual);
        Assert.Equal(3, actual.Length);
        Assert.Equal(0x0a, actual[0]);
        Assert.Equal(0x00, actual[1]);
        Assert.Equal(0x0d, actual[2]);
    }

    [Fact]
    public void ShouldReturnEmptyByteArrayWhenValueIsEmpty()
    {
        byte[] value = { };

        var actual = Extensions.TrimLeadingZeros(value);

        Assert.NotNull(actual);
        Assert.Empty(actual);
    }
}
