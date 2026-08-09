using System;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Responses;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp.Responses; //was previously: Renci.SshNet.Tests.Classes.Sftp.Responses;

public class StatVfsReplyInfoTest
{
    private Random _random;
    private uint _responseId;
    private ulong _bsize;
    private ulong _frsize;
    private ulong _blocks;
    private ulong _bfree;
    private ulong _bavail;
    private ulong _files;
    private ulong _ffree;
    private ulong _favail;
    private ulong _sid;
    private ulong _namemax;

    public StatVfsReplyInfoTest()
    {
        Init();
    }

    private void Init()
    {
        _random = new Random();
        _responseId = (uint)_random.Next(0, int.MaxValue);
        _bsize = (ulong)_random.Next(0, int.MaxValue);
        _frsize = (ulong)_random.Next(0, int.MaxValue);
        _blocks = (ulong)_random.Next(0, int.MaxValue);
        _bfree = (ulong)_random.Next(0, int.MaxValue);
        _bavail = (ulong)_random.Next(0, int.MaxValue);
        _files = (ulong)_random.Next(0, int.MaxValue);
        _ffree = (ulong)_random.Next(0, int.MaxValue);
        _favail = (ulong)_random.Next(0, int.MaxValue);
        _sid = (ulong)_random.Next(0, int.MaxValue);
        _namemax = (ulong)_random.Next(0, int.MaxValue);
    }

    [Fact]
    public void Constructor()
    {
        var target = new StatVfsReplyInfo();

        Assert.Null(target.Information);
    }

    [Fact]
    public void Load()
    {
        var sshDataStream = new SshDataStream(4 + 1 + 4 + 88);
        sshDataStream.Write(_responseId);
        sshDataStream.Write(_bsize);
        sshDataStream.Write(_frsize);
        sshDataStream.Write(_blocks);
        sshDataStream.Write(_bfree);
        sshDataStream.Write(_bavail);
        sshDataStream.Write(_files);
        sshDataStream.Write(_ffree);
        sshDataStream.Write(_favail);
        sshDataStream.Write(_sid);
        sshDataStream.Write((ulong)0x1);
        sshDataStream.Write(_namemax);

        var extendedReplyResponse = new SftpExtendedReplyResponse(SftpSession.MaximumSupportedVersion);
        extendedReplyResponse.Load(sshDataStream.ToArray());

        Assert.Equal(_responseId, extendedReplyResponse.ResponseId);

        var target = extendedReplyResponse.GetReply<StatVfsReplyInfo>();

        Assert.NotNull(target.Information);

        var information = target.Information;
        Assert.Equal(_bavail, information.AvailableBlocks);
        Assert.Equal(_favail, information.AvailableNodes);
        Assert.Equal(_frsize, information.BlockSize);
        Assert.Equal(_bsize, information.FileSystemBlockSize);
        Assert.Equal(_bfree, information.FreeBlocks);
        Assert.Equal(_ffree, information.FreeNodes);
        Assert.True(information.IsReadOnly);
        Assert.Equal(_namemax, information.MaxNameLenght);
        Assert.Equal(_sid, information.Sid);
        Assert.True(information.SupportsSetUid);
        Assert.Equal(_blocks, information.TotalBlocks);
        Assert.Equal(_files, information.TotalNodes);
    }
}
