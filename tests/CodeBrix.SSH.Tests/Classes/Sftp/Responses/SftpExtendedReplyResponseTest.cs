using System;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Responses;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp.Responses; //was previously: Renci.SshNet.Tests.Classes.Sftp.Responses;

public class SftpExtendedReplyResponseTest
{
    private Random _random;
    private uint _protocolVersion;
    private uint _responseId;

    public SftpExtendedReplyResponseTest()
    {
        Init();
    }

    private void Init()
    {
        _random = new Random();
        _protocolVersion = (uint)_random.Next(0, int.MaxValue);
        _responseId = (uint)_random.Next(0, int.MaxValue);
    }

    [Fact]
    public void Constructor()
    {
        var target = new SftpExtendedReplyResponse(_protocolVersion);

        Assert.Equal(_protocolVersion, target.ProtocolVersion);
        Assert.Equal((uint)0, target.ResponseId);
        Assert.Equal(SftpMessageTypes.ExtendedReply, target.SftpMessageType);
    }

    [Fact]
    public void Load()
    {
        var target = new SftpExtendedReplyResponse(_protocolVersion);

        var sshDataStream = new SshDataStream(4);
        sshDataStream.Write(_responseId);

        target.Load(sshDataStream.ToArray());

        Assert.Equal(_protocolVersion, target.ProtocolVersion);
        Assert.Equal(_responseId, target.ResponseId);
        Assert.Equal(SftpMessageTypes.ExtendedReply, target.SftpMessageType);
    }

    [Fact]
    public void GetReply_StatVfsReplyInfo()
    {
        var bsize = (ulong)_random.Next(0, int.MaxValue);
        var frsize = (ulong)_random.Next(0, int.MaxValue);
        var blocks = (ulong)_random.Next(0, int.MaxValue);
        var bfree = (ulong)_random.Next(0, int.MaxValue);
        var bavail = (ulong)_random.Next(0, int.MaxValue);
        var files = (ulong)_random.Next(0, int.MaxValue);
        var ffree = (ulong)_random.Next(0, int.MaxValue);
        var favail = (ulong)_random.Next(0, int.MaxValue);
        var sid = (ulong)_random.Next(0, int.MaxValue);
        var namemax = (ulong)_random.Next(0, int.MaxValue);

        var sshDataStream = new SshDataStream(4 + 1 + 4 + 88)
        {
            Position = 4 // skip 4 bytes for SSH packet length
        };
        sshDataStream.WriteByte((byte)SftpMessageTypes.Attrs);
        sshDataStream.Write(_responseId);
        sshDataStream.Write(bsize);
        sshDataStream.Write(frsize);
        sshDataStream.Write(blocks);
        sshDataStream.Write(bfree);
        sshDataStream.Write(bavail);
        sshDataStream.Write(files);
        sshDataStream.Write(ffree);
        sshDataStream.Write(favail);
        sshDataStream.Write(sid);
        sshDataStream.Write((ulong)0x2);
        sshDataStream.Write(namemax);

        var sshData = sshDataStream.ToArray();

        var target = new SftpExtendedReplyResponse(_protocolVersion);
        target.Load(sshData, 5, sshData.Length - 5);

        var reply = target.GetReply<StatVfsReplyInfo>();
        Assert.NotNull(reply);

        var information = reply.Information;
        Assert.NotNull(information);
        Assert.Equal(bavail, information.AvailableBlocks);
        Assert.Equal(favail, information.AvailableNodes);
        Assert.Equal(frsize, information.BlockSize);
        Assert.Equal(bsize, information.FileSystemBlockSize);
        Assert.Equal(bfree, information.FreeBlocks);
        Assert.Equal(ffree, information.FreeNodes);
        Assert.False(information.IsReadOnly);
        Assert.Equal(namemax, information.MaxNameLenght);
        Assert.Equal(sid, information.Sid);
        Assert.False(information.SupportsSetUid);
        Assert.Equal(blocks, information.TotalBlocks);
        Assert.Equal(files, information.TotalNodes);
    }
}
