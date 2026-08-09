using System;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Responses;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp.Responses; //was previously: Renci.SshNet.Tests.Classes.Sftp.Responses;

public class SftpAttrsResponseTest
{
    private Random _random;
    private uint _protocolVersion;
    private uint _responseId;

    public SftpAttrsResponseTest()
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
        var target = new SftpAttrsResponse(_protocolVersion);

        Assert.Null(target.Attributes);
        Assert.Equal(_protocolVersion, target.ProtocolVersion);
        Assert.Equal((uint)0, target.ResponseId);
        Assert.Equal(SftpMessageTypes.Attrs, target.SftpMessageType);
    }

    [Fact]
    public void Load()
    {
        var target = new SftpAttrsResponse(_protocolVersion);
        var attributes = CreateSftpFileAttributes();
        var attributesBytes = attributes.GetBytes();

        var sshDataStream = new SshDataStream(4 + attributesBytes.Length);
        sshDataStream.Write(_responseId);
        sshDataStream.Write(attributesBytes, 0, attributesBytes.Length);

        target.Load(sshDataStream.ToArray());

        Assert.NotNull(target.Attributes);
        Assert.Equal(_protocolVersion, target.ProtocolVersion);
        Assert.Equal(_responseId, target.ResponseId);
        Assert.Equal(SftpMessageTypes.Attrs, target.SftpMessageType);

        // check attributes in detail
        Assert.Equal(attributes.GroupId, target.Attributes.GroupId);
        Assert.Equal(attributes.LastWriteTime, target.Attributes.LastWriteTime);
        Assert.Equal(attributes.LastWriteTime, target.Attributes.LastWriteTime);
        Assert.Equal(attributes.UserId, target.Attributes.UserId);
    }

    private SftpFileAttributes CreateSftpFileAttributes()
    {
        var attributes = SftpFileAttributesBuilder.Empty;
        attributes.GroupId = _random.Next();
        attributes.LastAccessTime = new DateTime(2014, 8, 23, 17, 43, 50, DateTimeKind.Local);
        attributes.LastWriteTime = new DateTime(2013, 7, 22, 16, 40, 42, DateTimeKind.Local);
        attributes.UserId = _random.Next();
        return attributes;
    }
}
