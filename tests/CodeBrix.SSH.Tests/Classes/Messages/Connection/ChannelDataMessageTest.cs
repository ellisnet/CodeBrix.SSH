using System;
using System.Linq;
using CodeBrix.SSH.Abstractions;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Messages.Connection; //was previously: Renci.SshNet.Tests.Classes.Messages.Connection;

/// <summary>
/// This is a test class for ChannelDataMessageTest and is intended
/// to contain all ChannelDataMessageTest Unit Tests
/// </summary>
public class ChannelDataMessageTest : TestBase
{
    [Fact]
    public void DefaultConstructor()
    {
        var target = new ChannelDataMessage();

        Assert.Null(target.Data);
        Assert.Equal(0, target.Offset);
        Assert.Equal(0, target.Size);
    }

    [Fact]
    public void Constructor_LocalChannelNumberAndData()
    {
        var random = new Random();

        var localChannelNumber = (uint)random.Next(0, int.MaxValue);
        var data = new byte[3];

        var target = new ChannelDataMessage(localChannelNumber, data);

        Assert.Same(data, target.Data);
        Assert.Equal(0, target.Offset);
        Assert.Equal(data.Length, target.Size);
    }

    [Fact]
    public void Constructor_LocalChannelNumberAndData_ShouldThrowArgumentNullExceptionWhenDataIsNull()
    {
        var localChannelNumber = (uint)new Random().Next(0, int.MaxValue);
        const byte[] data = null;

        try
        {
            new ChannelDataMessage(localChannelNumber, data);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("data", ex.ParamName);
        }
    }

    [Fact]
    public void Constructor_LocalChannelNumberAndDataAndOffsetAndSize()
    {
        var localChannelNumber = (uint)new Random().Next(0, int.MaxValue);
        var data = new byte[4];
        const int offset = 2;
        const int size = 1;

        var target = new ChannelDataMessage(localChannelNumber, data, offset, size);

        Assert.Same(data, target.Data);
        Assert.Equal(offset, target.Offset);
        Assert.Equal(size, target.Size);
    }

    [Fact]
    public void Constructor_LocalChannelNumberAndDataAndOffsetAndSize_ShouldThrowArgumentNullExceptionWhenDataIsNull()
    {
        var localChannelNumber = (uint)new Random().Next(0, int.MaxValue);
        const byte[] data = null;
        const int offset = 0;
        const int size = 0;

        try
        {
            new ChannelDataMessage(localChannelNumber, data, offset, size);
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
        var random = new Random();

        var localChannelNumber = (uint)random.Next(0, int.MaxValue);
        var data = CryptoAbstraction.GenerateRandom(random.Next(10, 20));
        var offset = random.Next(0, data.Length - 1);
        var size = random.Next(0, data.Length - offset);

        var target = new ChannelDataMessage(localChannelNumber, data, offset, size);

        var bytes = target.GetBytes();

        var expectedBytesLength = 1; // Type
        expectedBytesLength += 4; // LocalChannelNumber
        expectedBytesLength += 4; // Data length
        expectedBytesLength += size; // Data

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal(target.MessageNumber, sshDataStream.ReadByte());
        Assert.Equal(localChannelNumber, sshDataStream.ReadUInt32());
        Assert.Equal((uint)size, sshDataStream.ReadUInt32());

        var actualData = new byte[size];
        _ = sshDataStream.Read(actualData, 0, size);
        Assert.True(actualData.SequenceEqual(data.Take(offset, size)));

        Assert.True(sshDataStream.IsEndOfData);
    }

    [Fact]
    public void Load()
    {
        var random = new Random();

        var localChannelNumber = (uint)random.Next(0, int.MaxValue);
        var data = CryptoAbstraction.GenerateRandom(random.Next(10, 20));

        var offset = random.Next(0, data.Length - 1);
        var size = random.Next(0, data.Length - offset);
        var channelDataMessage = new ChannelDataMessage(localChannelNumber, data, offset, size);
        var bytes = channelDataMessage.GetBytes();
        var target = new ChannelDataMessage();

        target.Load(bytes, 1, bytes.Length - 1); // skip message type

        Assert.Equal(data.Take(offset, size), target.Data.Take(target.Offset, target.Size));
    }
}
