using System;
using System.Linq;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Responses;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp.Responses; //was previously: Renci.SshNet.Tests.Classes.Sftp.Responses;

public class SftpDataResponseTest
{
    private Random _random;
    private uint _protocolVersion;
    private uint _responseId;
    private byte[] _data;

    public SftpDataResponseTest()
    {
        Init();
    }

    private void Init()
    {
        _random = new Random();
        _protocolVersion = (uint)_random.Next(0, int.MaxValue);
        _responseId = (uint)_random.Next(0, int.MaxValue);
        _data = new byte[_random.Next(10, 100)];
        _random.NextBytes(_data);
    }

    [Fact]
    public void Constructor()
    {
        var target = new SftpDataResponse(_protocolVersion);

        Assert.Null(target.Data);
        Assert.Equal(_protocolVersion, target.ProtocolVersion);
        Assert.Equal((uint)0, target.ResponseId);
        Assert.Equal(SftpMessageTypes.Data, target.SftpMessageType);
    }

    [Fact]
    public void Load()
    {
        var target = new SftpDataResponse(_protocolVersion);

        var sshDataStream = new SshDataStream(4 + _data.Length);
        sshDataStream.Write(_responseId);
        sshDataStream.Write((uint)_data.Length);
        sshDataStream.Write(_data, 0, _data.Length);

        var sshData = sshDataStream.ToArray();

        target.Load(sshData);

        Assert.NotNull(target.Data);
        Assert.True(target.Data.SequenceEqual(_data));
        Assert.Equal(_protocolVersion, target.ProtocolVersion);
        Assert.Equal(_responseId, target.ResponseId);
        Assert.Equal(SftpMessageTypes.Data, target.SftpMessageType);
    }
}
