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

namespace CodeBrix.SSH.Tests.Classes.Sftp.Requests.ExtendedRequests; //was previously: Renci.SshNet.Tests.Classes.Sftp.Requests.ExtendedRequests;

public class StatVfsRequestTest
{
    private uint _protocolVersion;
    private uint _requestId;
    private string _name;
    private string _path;
    private byte[] _pathBytes;
    private byte[] _nameBytes;
    private Encoding _encoding;

    public StatVfsRequestTest()
    {
        Init();
    }

    private void Init()
    {
        var random = new Random();

        _encoding = Encoding.Unicode;
        _protocolVersion = (uint)random.Next(0, int.MaxValue);
        _requestId = (uint)random.Next(0, int.MaxValue);
        _path = random.Next().ToString(CultureInfo.InvariantCulture);
        _pathBytes = _encoding.GetBytes(_path);

        _name = "statvfs@openssh.com";
        _nameBytes = Encoding.UTF8.GetBytes(_name);
    }

    [Fact]
    public void Constructor()
    {
        var request = new StatVfsRequest(_protocolVersion, _requestId, _path, _encoding, null, null);

        Assert.Same(_encoding, request.Encoding);
        Assert.Equal(_name, request.Name);
        Assert.Equal(_path, request.Path);
        Assert.Equal(_protocolVersion, request.ProtocolVersion);
        Assert.Equal(_requestId, request.RequestId);
        Assert.Equal(SftpMessageTypes.Extended, request.SftpMessageType);
    }

    [Fact]
    public void Complete_SftpStatusResponse()
    {
        IList<SftpStatusResponse> statusActionInvocations = new List<SftpStatusResponse>();
        IList<SftpExtendedReplyResponse> extendedReplyActionInvocations = new List<SftpExtendedReplyResponse>();

        Action<SftpExtendedReplyResponse> extendedAction = extendedReplyActionInvocations.Add;
        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        var statusResponse = new SftpStatusResponse(_protocolVersion);

        var request = new StatVfsRequest(_protocolVersion, _requestId, _path, _encoding, extendedAction, statusAction);

        request.Complete(statusResponse);

        Assert.Single(statusActionInvocations);
        Assert.Same(statusResponse, statusActionInvocations[0]);
        Assert.Empty(extendedReplyActionInvocations);
    }

    [Fact]
    public void Complete_SftpExtendedReplyResponse()
    {
        IList<SftpStatusResponse> statusActionInvocations = new List<SftpStatusResponse>();
        IList<SftpExtendedReplyResponse> extendedReplyActionInvocations = new List<SftpExtendedReplyResponse>();

        Action<SftpExtendedReplyResponse> extendedAction = extendedReplyActionInvocations.Add;
        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        var extendedReplyResponse = new SftpExtendedReplyResponse(_protocolVersion);

        var request = new StatVfsRequest(_protocolVersion, _requestId, _path, _encoding, extendedAction, statusAction);

        request.Complete(extendedReplyResponse);

        Assert.Empty(statusActionInvocations);
        Assert.Single(extendedReplyActionInvocations);
        Assert.Same(extendedReplyResponse, extendedReplyActionInvocations[0]);
    }

    [Fact]
    public void GetBytes()
    {
        var request = new StatVfsRequest(_protocolVersion, _requestId, _path, _encoding, null, null);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 4; // Length
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // RequestId
        expectedBytesLength += 4; // Name length
        expectedBytesLength += _nameBytes.Length; // Name
        expectedBytesLength += 4; // Path length
        expectedBytesLength += _pathBytes.Length; // Path

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal((uint)bytes.Length - 4, sshDataStream.ReadUInt32());
        Assert.Equal((byte)SftpMessageTypes.Extended, sshDataStream.ReadByte());
        Assert.Equal(_requestId, sshDataStream.ReadUInt32());
        Assert.Equal((uint)_nameBytes.Length, sshDataStream.ReadUInt32());

        var actualNameBytes = new byte[_nameBytes.Length];
        _ = sshDataStream.Read(actualNameBytes, 0, actualNameBytes.Length);
        Assert.True(_nameBytes.SequenceEqual(actualNameBytes));

        Assert.Equal((uint)_pathBytes.Length, sshDataStream.ReadUInt32());

        var actualPath = new byte[_pathBytes.Length];
        _ = sshDataStream.Read(actualPath, 0, actualPath.Length);
        Assert.True(_pathBytes.SequenceEqual(actualPath));

        Assert.True(sshDataStream.IsEndOfData);
    }
}
