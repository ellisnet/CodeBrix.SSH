using CodeBrix.SSH.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Common; //was previously: Renci.SshNet.Tests.Classes.Common;

public class ExtensionsTest_ToBigInteger2
{
    [Fact]
    public void ShouldNotAppendZero()
    {
        byte[] value = { 0x0a, 0x0d };

        var actual = value.ToBigInteger2().ToByteArray(isBigEndian: true);

        Assert.NotNull(actual);
        Assert.Equal(2, actual.Length);
        Assert.Equal(0x0a, actual[0]);
        Assert.Equal(0x0d, actual[1]);
    }

    [Fact]
    public void ShouldAppendZero()
    {
        byte[] value = { 0xff, 0x0a, 0x0d };

        var actual = value.ToBigInteger2().ToByteArray(isBigEndian: true);

        Assert.NotNull(actual);
        Assert.Equal(4, actual.Length);
        Assert.Equal(0x00, actual[0]);
        Assert.Equal(0xff, actual[1]);
        Assert.Equal(0x0a, actual[2]);
        Assert.Equal(0x0d, actual[3]);
    }
}
