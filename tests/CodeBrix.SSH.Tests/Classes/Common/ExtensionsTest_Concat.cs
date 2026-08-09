using System;
using CodeBrix.SSH.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Common; //was previously: Renci.SshNet.Tests.Classes.Common;

public class ExtensionsTest_Concat
{
    private Random _random;

    public ExtensionsTest_Concat()
    {
        Init();
    }

    private void Init()
    {
        _random = new Random();
    }

    [Fact]
    public void ShouldReturnSecondWhenFirstIsEmpty()
    {
        var first = Array.Empty<byte>();
        var second = CreateBuffer(16);

        var actual = Extensions.Concat(first, second);

        Assert.NotNull(actual);
        Assert.Equal(second, actual);
    }

    [Fact]
    public void ShouldReturnSecondWhenFirstIsNull()
    {
        const byte[] first = null;
        var second = CreateBuffer(16);

        var actual = Extensions.Concat(first, second);

        Assert.NotNull(actual);
        Assert.Equal(second, actual);
    }

    [Fact]
    public void ShouldReturnFirstWhenSecondIsEmpty()
    {
        var first = CreateBuffer(16);
        var second = Array.Empty<byte>();

        var actual = Extensions.Concat(first, second);

        Assert.NotNull(actual);
        Assert.Equal(first, actual);
    }

    [Fact]
    public void ShouldReturnFirstWhenSecondIsNull()
    {
        var first = CreateBuffer(16);
        const byte[] second = null;

        var actual = Extensions.Concat(first, second);

        Assert.NotNull(actual);
        Assert.Equal(first, actual);
    }

    [Fact]
    public void ShouldReturnNullWhenFirstAndSecondAreNull()
    {
        const byte[] first = null;
        const byte[] second = null;

        var actual = Extensions.Concat(first, second);

        Assert.Null(actual);
    }

    [Fact]
    public void ShouldConcatSecondToFirstWhenBothAreNotEmpty()
    {
        var first = CreateBuffer(4);
        var second = CreateBuffer(2);

        var actual = Extensions.Concat(first, second);

        Assert.NotNull(actual);
        Assert.Equal(first.Length + second.Length, actual.Length);
        Assert.Equal(first[0], actual[0]);
        Assert.Equal(first[1], actual[1]);
        Assert.Equal(first[2], actual[2]);
        Assert.Equal(first[3], actual[3]);
        Assert.Equal(second[0], actual[4]);
        Assert.Equal(second[1], actual[5]);
    }

    private byte[] CreateBuffer(int length)
    {
        var buffer = new byte[length];
        _random.NextBytes(buffer);
        return buffer;
    }
}
