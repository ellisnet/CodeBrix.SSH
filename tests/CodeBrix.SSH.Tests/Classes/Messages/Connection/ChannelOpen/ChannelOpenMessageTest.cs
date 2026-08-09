using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Messages.Connection; //was previously: Renci.SshNet.Tests.Classes.Messages.Connection;

public class ChannelOpenMessageTest
{
    private Random _random;
    private Encoding _ascii;

    public ChannelOpenMessageTest()
    {
        Init();
    }

    private void Init()
    {
        _random = new Random();
        _ascii = Encoding.ASCII;
    }

    [Fact]
    public void DefaultConstructor()
    {
        var target = new ChannelOpenMessage();

        Assert.Null(target.ChannelType);
        Assert.Null(target.Info);
        Assert.Equal(default, target.InitialWindowSize);
        Assert.Equal(default, target.LocalChannelNumber);
        Assert.Equal(default, target.MaximumPacketSize);
    }

    [Fact]
    public void Constructor_LocalChannelNumberAndInitialWindowSizeAndMaximumPacketSizeAndInfo()
    {
        var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
        var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
        var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
        var info = new DirectTcpipChannelInfo("host", 22, "originator", 25);

        var target = new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);

        Assert.Equal(info.ChannelType, _ascii.GetString(target.ChannelType));
        Assert.Same(info, target.Info);
        Assert.Equal(initialWindowSize, target.InitialWindowSize);
        Assert.Equal(localChannelNumber, target.LocalChannelNumber);
        Assert.Equal(maximumPacketSize, target.MaximumPacketSize);
    }

    [Fact]
    public void Constructor_LocalChannelNumberAndInitialWindowSizeAndMaximumPacketSizeAndInfo_ShouldThrowArgumentNullExceptionWhenInfoIsNull()
    {
        var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
        var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
        var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
        ChannelOpenInfo info = null;

        try
        {
            new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("info", ex.ParamName);
        }
    }

    [Fact]
    public void GetBytes()
    {
        var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
        var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
        var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
        var info = new DirectTcpipChannelInfo("host", 22, "originator", 25);
        var infoBytes = info.GetBytes();
        var target = new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);

        var bytes = target.GetBytes();

        var expectedBytesLength = 1; // Type
        expectedBytesLength += 4; // ChannelType length
        expectedBytesLength += target.ChannelType.Length; // ChannelType
        expectedBytesLength += 4; // LocalChannelNumber
        expectedBytesLength += 4; // InitialWindowSize
        expectedBytesLength += 4; // MaximumPacketSize
        expectedBytesLength += infoBytes.Length; // Info

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal(target.MessageNumber, sshDataStream.ReadByte());

        var actualChannelTypeLength = sshDataStream.ReadUInt32();
        Assert.Equal((uint)target.ChannelType.Length, actualChannelTypeLength);

        var actualChannelType = new byte[actualChannelTypeLength];
        _ = sshDataStream.Read(actualChannelType, 0, (int)actualChannelTypeLength);
        Assert.True(target.ChannelType.SequenceEqual(actualChannelType));

        Assert.Equal(localChannelNumber, sshDataStream.ReadUInt32());
        Assert.Equal(initialWindowSize, sshDataStream.ReadUInt32());
        Assert.Equal(maximumPacketSize, sshDataStream.ReadUInt32());

        var actualInfo = new byte[infoBytes.Length];
        _ = sshDataStream.Read(actualInfo, 0, actualInfo.Length);
        Assert.True(infoBytes.SequenceEqual(actualInfo));

        Assert.True(sshDataStream.IsEndOfData);
    }

    [Fact]
    public void Load_DirectTcpipChannelInfo()
    {
        var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
        var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
        var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
        var info = new DirectTcpipChannelInfo("host", 22, "originator", 25);
        var target = new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);
        var bytes = target.GetBytes();

        target.Load(bytes, 1, bytes.Length - 1); // skip message type

        Assert.Equal(info.ChannelType, _ascii.GetString(target.ChannelType));
        Assert.NotNull(target.Info);
        Assert.Equal(initialWindowSize, target.InitialWindowSize);
        Assert.Equal(localChannelNumber, target.LocalChannelNumber);
        Assert.Equal(maximumPacketSize, target.MaximumPacketSize);

        var directTcpChannelInfo = target.Info as DirectTcpipChannelInfo;
        Assert.NotNull(directTcpChannelInfo);
        Assert.Equal(info.ChannelType, directTcpChannelInfo.ChannelType);
        Assert.Equal(info.HostToConnect, directTcpChannelInfo.HostToConnect);
        Assert.Equal(info.OriginatorAddress, directTcpChannelInfo.OriginatorAddress);
        Assert.Equal(info.OriginatorPort, directTcpChannelInfo.OriginatorPort);
        Assert.Equal(info.PortToConnect, directTcpChannelInfo.PortToConnect);
    }

    [Fact]
    public void Load_ForwardedTcpipChannelInfo()
    {
        var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
        var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
        var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
        var info = new ForwardedTcpipChannelInfo("connected", 25, "originator", 21);
        var target = new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);
        var bytes = target.GetBytes();

        target.Load(bytes, 1, bytes.Length - 1); // skip message type

        Assert.Equal(info.ChannelType, _ascii.GetString(target.ChannelType));
        Assert.NotNull(target.Info);
        Assert.Equal(initialWindowSize, target.InitialWindowSize);
        Assert.Equal(localChannelNumber, target.LocalChannelNumber);
        Assert.Equal(maximumPacketSize, target.MaximumPacketSize);

        var forwardedTcpipChannelInfo = target.Info as ForwardedTcpipChannelInfo;
        Assert.NotNull(forwardedTcpipChannelInfo);
        Assert.Equal(info.ChannelType, forwardedTcpipChannelInfo.ChannelType);
        Assert.Equal(info.ConnectedAddress, forwardedTcpipChannelInfo.ConnectedAddress);
        Assert.Equal(info.ConnectedPort, forwardedTcpipChannelInfo.ConnectedPort);
        Assert.Equal(info.OriginatorAddress, forwardedTcpipChannelInfo.OriginatorAddress);
        Assert.Equal(info.OriginatorPort, forwardedTcpipChannelInfo.OriginatorPort);
    }

    [Fact]
    public void Load_SessionChannelOpenInfo()
    {
        var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
        var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
        var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
        var info = new SessionChannelOpenInfo();
        var target = new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);
        var bytes = target.GetBytes();

        target.Load(bytes, 1, bytes.Length - 1); // skip message type

        Assert.Equal(info.ChannelType, _ascii.GetString(target.ChannelType));
        Assert.NotNull(target.Info);
        Assert.Equal(initialWindowSize, target.InitialWindowSize);
        Assert.Equal(localChannelNumber, target.LocalChannelNumber);
        Assert.Equal(maximumPacketSize, target.MaximumPacketSize);

        var sessionChannelOpenInfo = target.Info as SessionChannelOpenInfo;
        Assert.NotNull(sessionChannelOpenInfo);
        Assert.Equal(info.ChannelType, sessionChannelOpenInfo.ChannelType);
    }

    [Fact]
    public void Load_X11ChannelOpenInfo()
    {
        var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
        var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
        var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
        var info = new X11ChannelOpenInfo("address", 26);
        var target = new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);
        var bytes = target.GetBytes();

        target.Load(bytes, 1, bytes.Length - 1); // skip message type

        Assert.Equal(info.ChannelType, _ascii.GetString(target.ChannelType));
        Assert.NotNull(target.Info);
        Assert.Equal(initialWindowSize, target.InitialWindowSize);
        Assert.Equal(localChannelNumber, target.LocalChannelNumber);
        Assert.Equal(maximumPacketSize, target.MaximumPacketSize);

        var x11ChannelOpenInfo = target.Info as X11ChannelOpenInfo;
        Assert.NotNull(x11ChannelOpenInfo);
        Assert.Equal(info.ChannelType, x11ChannelOpenInfo.ChannelType);
        Assert.Equal(info.OriginatorAddress, x11ChannelOpenInfo.OriginatorAddress);
        Assert.Equal(info.OriginatorPort, x11ChannelOpenInfo.OriginatorPort);
    }

    [Fact]
    public void Load_ShouldThrowNotSupportedExceptionWhenChannelTypeIsNotSupported()
    {
        var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
        var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
        var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
        var channelName = "dunno_" + _random.Next().ToString(CultureInfo.InvariantCulture);
        var channelType = _ascii.GetBytes(channelName);
        var target = new ChannelOpenMessage();

        var sshDataStream = new SshDataStream(1 + 4 + channelType.Length + 4 + 4 + 4);
        sshDataStream.WriteByte(target.MessageNumber);
        sshDataStream.Write((uint)channelType.Length);
        sshDataStream.Write(channelType, 0, channelType.Length);
        sshDataStream.Write(localChannelNumber);
        sshDataStream.Write(initialWindowSize);
        sshDataStream.Write(maximumPacketSize);
        var bytes = sshDataStream.ToArray();

        try
        {
            target.Load(bytes, 1, bytes.Length - 1); // skip message type
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (NotSupportedException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal(string.Format("Channel type '{0}' is not supported.", channelName), ex.Message);
        }
    }
}
