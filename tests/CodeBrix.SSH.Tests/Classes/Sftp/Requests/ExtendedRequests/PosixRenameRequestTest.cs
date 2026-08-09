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

public class PosixRenameRequestTest
{
    private uint _protocolVersion;
    private uint _requestId;
    private string _name;
    private string _oldPath;
    private byte[] _oldPathBytes;
    private string _newPath;
    private byte[] _newPathBytes;
    private byte[] _nameBytes;
    private Encoding _encoding;

    public PosixRenameRequestTest()
    {
        Init();
    }

    private void Init()
    {
        var random = new Random();

        _encoding = Encoding.Unicode;
        _protocolVersion = (uint)random.Next(0, int.MaxValue);
        _requestId = (uint)random.Next(0, int.MaxValue);
        _oldPath = random.Next().ToString(CultureInfo.InvariantCulture);
        _oldPathBytes = _encoding.GetBytes(_oldPath);
        _newPath = random.Next().ToString(CultureInfo.InvariantCulture);
        _newPathBytes = _encoding.GetBytes(_newPath);

        _name = "posix-rename@openssh.com";
        _nameBytes = Encoding.UTF8.GetBytes(_name);
    }

    [Fact]
    public void Constructor()
    {
        var request = new PosixRenameRequest(_protocolVersion, _requestId, _oldPath, _newPath, _encoding, null);

        Assert.Same(_encoding, request.Encoding);
        Assert.Equal(_name, request.Name);
        Assert.Equal(_newPath, request.NewPath);
        Assert.Equal(_oldPath, request.OldPath);
        Assert.Equal(_protocolVersion, request.ProtocolVersion);
        Assert.Equal(_requestId, request.RequestId);
        Assert.Equal(SftpMessageTypes.Extended, request.SftpMessageType);
    }

    [Fact]
    public void Complete_SftpStatusResponse()
    {
        IList<SftpStatusResponse> statusActionInvocations = new List<SftpStatusResponse>();

        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        var statusResponse = new SftpStatusResponse(_protocolVersion);

        var request = new PosixRenameRequest(_protocolVersion, _requestId, _oldPath, _newPath, _encoding, statusAction);

        request.Complete(statusResponse);

        Assert.Single(statusActionInvocations);
        Assert.Same(statusResponse, statusActionInvocations[0]);
    }

    [Fact]
    public void GetBytes()
    {
        var request = new PosixRenameRequest(_protocolVersion, _requestId, _oldPath, _newPath, _encoding, null);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 4; // Length
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // RequestId
        expectedBytesLength += 4; // Name length
        expectedBytesLength += _nameBytes.Length; // Name
        expectedBytesLength += 4; // OldPath length
        expectedBytesLength += _oldPathBytes.Length; // OldPath
        expectedBytesLength += 4; // NewPath length
        expectedBytesLength += _newPathBytes.Length; // NewPath

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal((uint)bytes.Length - 4, sshDataStream.ReadUInt32());
        Assert.Equal((byte)SftpMessageTypes.Extended, sshDataStream.ReadByte());
        Assert.Equal(_requestId, sshDataStream.ReadUInt32());
        Assert.Equal((uint)_nameBytes.Length, sshDataStream.ReadUInt32());

        var actualNameBytes = new byte[_nameBytes.Length];
        _ = sshDataStream.Read(actualNameBytes, 0, actualNameBytes.Length);
        Assert.True(_nameBytes.SequenceEqual(actualNameBytes));

        Assert.Equal((uint)_oldPathBytes.Length, sshDataStream.ReadUInt32());

        var actualOldPath = new byte[_oldPathBytes.Length];
        _ = sshDataStream.Read(actualOldPath, 0, actualOldPath.Length);
        Assert.True(_oldPathBytes.SequenceEqual(actualOldPath));

        Assert.Equal((uint)_newPathBytes.Length, sshDataStream.ReadUInt32());

        var actualNewPath = new byte[_newPathBytes.Length];
        _ = sshDataStream.Read(actualNewPath, 0, actualNewPath.Length);
        Assert.True(_newPathBytes.SequenceEqual(actualNewPath));

        Assert.True(sshDataStream.IsEndOfData);
    }
}
