using System;
using System.Linq;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Responses;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp.Responses; //was previously: Renci.SshNet.Tests.Classes.Sftp.Responses;

public class SftpHandleResponseTest
{
    private Random _random;
    private uint _protocolVersion;
    private uint _responseId;
    private byte[] _handle;

    public SftpHandleResponseTest()
    {
        Init();
    }

    private void Init()
    {
        _random = new Random();
        _protocolVersion = (uint)_random.Next(0, int.MaxValue);
        _responseId = (uint)_random.Next(0, int.MaxValue);
        _handle = new byte[_random.Next(1, 10)];
        _random.NextBytes(_handle);
    }

    [Fact]
    public void Constructor()
    {
        var target = new SftpHandleResponse(_protocolVersion);

        Assert.Null(target.Handle);
        Assert.Equal(_protocolVersion, target.ProtocolVersion);
        Assert.Equal((uint)0, target.ResponseId);
        Assert.Equal(SftpMessageTypes.Handle, target.SftpMessageType);
    }

    [Fact]
    public void Load()
    {
        var target = new SftpHandleResponse(_protocolVersion);

        var sshDataStream = new SshDataStream(4 + _handle.Length);
        sshDataStream.Write(_responseId);
        sshDataStream.Write((uint)_handle.Length);
        sshDataStream.Write(_handle, 0, _handle.Length);

        target.Load(sshDataStream.ToArray());

        Assert.NotNull(target.Handle);
        Assert.True(target.Handle.SequenceEqual(_handle));
        Assert.Equal(_protocolVersion, target.ProtocolVersion);
        Assert.Equal(_responseId, target.ResponseId);
        Assert.Equal(SftpMessageTypes.Handle, target.SftpMessageType);
    }
}
