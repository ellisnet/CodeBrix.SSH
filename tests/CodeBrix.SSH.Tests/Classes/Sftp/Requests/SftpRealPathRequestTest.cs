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

public class SftpRealPathRequestTest
{
    private uint _protocolVersion;
    private uint _requestId;
    private Encoding _encoding;
    private string _path;
    private byte[] _pathBytes;

    public SftpRealPathRequestTest()
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
        var statusActionInvocations = new List<SftpStatusResponse>();
        var nameActionInvocations = new List<SftpNameResponse>();

        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        Action<SftpNameResponse> nameAction = nameActionInvocations.Add;

        var request = new SftpRealPathRequest(
            _protocolVersion,
            _requestId,
            _path,
            _encoding,
            nameAction,
            statusAction);

        Assert.Same(_encoding, request.Encoding);
        Assert.Equal(_path, request.Path);
        Assert.Equal(_protocolVersion, request.ProtocolVersion);
        Assert.Equal(_requestId, request.RequestId);
        Assert.Equal(SftpMessageTypes.RealPath, request.SftpMessageType);
    }

    [Fact]
    public void Complete_SftpNameResponse()
    {
        var statusActionInvocations = new List<SftpStatusResponse>();
        var nameActionInvocations = new List<SftpNameResponse>();

        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        Action<SftpNameResponse> nameAction = nameActionInvocations.Add;
        var nameResponse = new SftpNameResponse(_protocolVersion, Encoding.Unicode);

        var request = new SftpRealPathRequest(_protocolVersion, _requestId, _path, _encoding, nameAction, statusAction);

        request.Complete(nameResponse);

        Assert.Empty(statusActionInvocations);
        Assert.Single(nameActionInvocations);
        Assert.Same(nameResponse, nameActionInvocations[0]);
    }

    [Fact]
    public void Complete_SftpStatusResponse()
    {
        var statusActionInvocations = new List<SftpStatusResponse>();
        var nameActionInvocations = new List<SftpNameResponse>();

        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        Action<SftpNameResponse> nameAction = nameActionInvocations.Add;
        var statusResponse = new SftpStatusResponse(_protocolVersion);

        var request = new SftpRealPathRequest(
            _protocolVersion,
            _requestId,
            _path,
            _encoding,
            nameAction,
            statusAction);

        request.Complete(statusResponse);

        Assert.Single(statusActionInvocations);
        Assert.Same(statusResponse, statusActionInvocations[0]);
        Assert.Empty(nameActionInvocations);
    }

    [Fact]
    public void GetBytes()
    {
        var statusActionInvocations = new List<SftpStatusResponse>();
        var nameActionInvocations = new List<SftpNameResponse>();
        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        Action<SftpNameResponse> nameAction = nameActionInvocations.Add;
        var request = new SftpRealPathRequest(
            _protocolVersion,
            _requestId,
            _path,
            _encoding,
            nameAction,
            statusAction);

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
        Assert.Equal((byte)SftpMessageTypes.RealPath, sshDataStream.ReadByte());
        Assert.Equal(_requestId, sshDataStream.ReadUInt32());

        Assert.Equal((uint)_pathBytes.Length, sshDataStream.ReadUInt32());
        var actualPath = new byte[_pathBytes.Length];
        _ = sshDataStream.Read(actualPath, 0, actualPath.Length);
        Assert.True(_pathBytes.SequenceEqual(actualPath));

        Assert.True(sshDataStream.IsEndOfData);
    }
}
