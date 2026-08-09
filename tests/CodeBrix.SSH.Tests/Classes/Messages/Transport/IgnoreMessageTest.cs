using System;
using System.Linq;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Messages.Transport; //was previously: Renci.SshNet.Tests.Classes.Messages.Transport;

public class IgnoreMessageTest
{
    private Random _random;
    private byte[] _data;

    public IgnoreMessageTest()
    {
        Init();
    }

    private void Init()
    {
        _random = new Random();
        _data = new byte[_random.Next(1, 10)];
        _random.NextBytes(_data);
    }

    [Fact]
    public void DefaultConstructor()
    {
        var target = new IgnoreMessage();
        Assert.NotNull(target.Data);
        Assert.Empty(target.Data);
    }

    [Fact]
    public void Constructor_Data()
    {
        var target = new IgnoreMessage(_data);
        Assert.Same(_data, target.Data);
    }

    [Fact]
    public void Constructor_Data_ShouldThrowArgumentNullExceptionWhenDataIsNull()
    {
        const byte[] data = null;

        try
        {
            new IgnoreMessage(data);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("data", ex.ParamName);
        }
    }

    [Fact]
    public void GetBytes()
    {
        var request = new IgnoreMessage(_data);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // Data length
        expectedBytesLength += _data.Length; // Data

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal(request.MessageNumber, sshDataStream.ReadByte());
        Assert.Equal((uint)_data.Length, sshDataStream.ReadUInt32());

        var actualData = new byte[_data.Length];
        _ = sshDataStream.Read(actualData, 0, actualData.Length);
        Assert.True(_data.SequenceEqual(actualData));

        Assert.True(sshDataStream.IsEndOfData);
    }

    [Fact]
    public void Load_IgnoresData()
    {
        var ignoreMessage = new IgnoreMessage(_data);
        var bytes = ignoreMessage.GetBytes();
        var target = new IgnoreMessage();

        target.Load(bytes, 1, bytes.Length - 1);

        Assert.NotNull(target.Data);
        Assert.Empty(target.Data);
    }
}
