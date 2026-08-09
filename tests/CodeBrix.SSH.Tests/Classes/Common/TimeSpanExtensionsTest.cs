using System;
using CodeBrix.SSH.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Common; //was previously: Renci.SshNet.Tests.Classes.Common;

public class TimeSpanExtensionsTest
{
    [Fact]
    public void AsTimeout_ValidTimeSpan_ReturnsExpectedMilliseconds()
    {
        var timeSpan = TimeSpan.FromSeconds(10);

        var timeout = timeSpan.AsTimeout();

        Assert.Equal(10000, timeout);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData((double)int.MaxValue + 1)]
    public void AsTimeout_InvalidTimeSpan_ThrowsArgumentOutOfRangeException(double milliseconds)
    {
        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => timeSpan.AsTimeout());

        Assert.Contains("The timeout must represent a value between -1 and Int32.MaxValue milliseconds, inclusive.", ex.Message, StringComparison.Ordinal);

        Assert.Equal(nameof(timeSpan), ex.ParamName);
    }

    [Fact]
    public void EnsureValidTimeout_ValidTimeSpan_DoesNotThrow()
    {
        var timeSpan = TimeSpan.FromSeconds(5);

        timeSpan.EnsureValidTimeout();
    }

    [Theory]
    [InlineData(-2)]
    [InlineData((double)int.MaxValue + 1)]
    public void EnsureValidTimeout_InvalidTimeSpan_ThrowsArgumentOutOfRangeException(double milliseconds)
    {
        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => timeSpan.EnsureValidTimeout());

        Assert.Contains("The timeout must represent a value between -1 and Int32.MaxValue milliseconds, inclusive.", ex.Message, StringComparison.Ordinal);

        Assert.Equal(nameof(timeSpan), ex.ParamName);
    }
}
