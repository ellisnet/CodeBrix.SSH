using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Requests;
using CodeBrix.SSH.Sftp.Responses;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp.Requests; //was previously: Renci.SshNet.Tests.Classes.Sftp.Requests;

public class SftpStatRequestTest
{
    private uint _protocolVersion;
    private uint _requestId;
    private Encoding _encoding;
    private string _path;
    private byte[] _pathBytes;

    public SftpStatRequestTest()
    {
        Init();
    }

    private void Init()
    {
        var random = new Random();

        _protocolVersion = (uint)random.Next(0, int.MaxValue);
        _requestId = (uint)random.Next(0, int.MaxValue);
        _encoding = Encoding.Unicode;
        _path = random.Next().ToString(CultureInfo.InvariantCulture);
        _pathBytes = _encoding.GetBytes(_path);
    }

    [Fact]
    public void Constructor()
    {
        var request = new SftpStatRequest(_protocolVersion, _requestId, _path, _encoding, null, null);

        Assert.Same(_encoding, request.Encoding);
        Assert.Equal(_path, request.Path);
        Assert.Equal(_protocolVersion, request.ProtocolVersion);
        Assert.Equal(_requestId, request.RequestId);
        Assert.Equal(SftpMessageTypes.Stat, request.SftpMessageType);
    }

    [Fact]
    public void Complete_SftpAttrsResponse()
    {
        var statusActionInvocations = new List<SftpStatusResponse>();
        var attrsActionInvocations = new List<SftpAttrsResponse>();
        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        Action<SftpAttrsResponse> attrsAction = attrsActionInvocations.Add;
        var attrsResponse = new SftpAttrsResponse(_protocolVersion);

        var request = new SftpStatRequest(_protocolVersion, _requestId, _path, _encoding, attrsAction, statusAction);

        request.Complete(attrsResponse);

        Assert.Empty(statusActionInvocations);
        Assert.Single(attrsActionInvocations);
        Assert.Same(attrsResponse, attrsActionInvocations[0]);
    }

    [Fact]
    public void Complete_SftpStatusResponse()
    {
        var statusActionInvocations = new List<SftpStatusResponse>();
        var attrsActionInvocations = new List<SftpAttrsResponse>();
        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        Action<SftpAttrsResponse> attrsAction = attrsActionInvocations.Add;
        var statusResponse = new SftpStatusResponse(_protocolVersion);

        var request = new SftpStatRequest(_protocolVersion, _requestId, _path, _encoding, attrsAction, statusAction);

        request.Complete(statusResponse);

        Assert.Single(statusActionInvocations);
        Assert.Empty(attrsActionInvocations);
        Assert.Same(statusResponse, statusActionInvocations[0]);
    }

    [Fact]
    public void GetBytes()
    {
        var request = new SftpStatRequest(_protocolVersion, _requestId, _path, _encoding, null, null);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 4; // Length
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // RequestId
        expectedBytesLength += 4; // Path length
        expectedBytesLength += _pathBytes.Length; // Path

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal((uint)bytes.Length - 4, sshDataStream.ReadUInt32());
        Assert.Equal((byte)SftpMessageTypes.Stat, sshDataStream.ReadByte());
        Assert.Equal(_requestId, sshDataStream.ReadUInt32());

        Assert.Equal((uint)_pathBytes.Length, sshDataStream.ReadUInt32());
        var actualPath = new byte[_pathBytes.Length];
        _ = sshDataStream.Read(actualPath, 0, actualPath.Length);
        Assert.True(_pathBytes.SequenceEqual(actualPath));

        Assert.True(sshDataStream.IsEndOfData);
    }
}
