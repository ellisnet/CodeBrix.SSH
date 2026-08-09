using CodeBrix.SSH.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Common; //was previously: Renci.SshNet.Tests.Classes.Common;

public class ExtensionsTest_Pad
{
    [Fact]
    public void ShouldReturnNotPadded()
    {
        byte[] value = { 0x0a, 0x0d };
        var padded = value.Pad(2);
        Assert.Equal(value, padded);
        Assert.Equal(value.Length, padded.Length);
    }

    [Fact]
    public void ShouldReturnPadded()
    {
        byte[] value = { 0x0a, 0x0d };
        var padded = value.Pad(3);
        Assert.Equal(value.Length + 1, padded.Length);
        Assert.Equal(0x00, padded[0]);
        Assert.Equal(0x0a, padded[1]);
        Assert.Equal(0x0d, padded[2]);
    }
}
