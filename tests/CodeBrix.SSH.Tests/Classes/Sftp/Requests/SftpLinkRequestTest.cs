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

public class SftpLinkRequestTest
{
    private uint _protocolVersion;
    private uint _requestId;
    private string _newLinkPath;
    private byte[] _newLinkPathBytes;
    private string _existingPath;
    private byte[] _existingPathBytes;

    public SftpLinkRequestTest()
    {
        Init();
    }

    private void Init()
    {
        var random = new Random();

        _protocolVersion = (uint)random.Next(0, int.MaxValue);
        _requestId = (uint)random.Next(0, int.MaxValue);
        _newLinkPath = random.Next().ToString(CultureInfo.InvariantCulture);
        _newLinkPathBytes = Encoding.UTF8.GetBytes(_newLinkPath);
        _existingPath = random.Next().ToString(CultureInfo.InvariantCulture);
        _existingPathBytes = Encoding.UTF8.GetBytes(_existingPath);
    }

    [Fact]
    public void Constructor()
    {
        var request = new SftpLinkRequest(_protocolVersion, _requestId, _newLinkPath, _existingPath, true, null);

        Assert.Equal(_existingPath, request.ExistingPath);
        Assert.True(request.IsSymLink);
        Assert.Equal(_newLinkPath, request.NewLinkPath);
        Assert.Equal(_protocolVersion, request.ProtocolVersion);
        Assert.Equal(_requestId, request.RequestId);
        Assert.Equal(SftpMessageTypes.Link, request.SftpMessageType);

        request = new SftpLinkRequest(_protocolVersion, _requestId, _newLinkPath, _existingPath, false, null);

        Assert.False(request.IsSymLink);
    }

    [Fact]
    public void Complete_SftpStatusResponse()
    {
        var statusActionInvocations = new List<SftpStatusResponse>();

        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        var statusResponse = new SftpStatusResponse(_protocolVersion);

        var request = new SftpLinkRequest(_protocolVersion, _requestId, _newLinkPath, _existingPath, true, statusAction);

        request.Complete(statusResponse);

        Assert.Single(statusActionInvocations);
        Assert.Same(statusResponse, statusActionInvocations[0]);
    }

    [Fact]
    public void GetBytes()
    {
        var request = new SftpLinkRequest(_protocolVersion, _requestId, _newLinkPath, _existingPath, true, null);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 4; // Length
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // RequestId
        expectedBytesLength += 4; // NewLinkPath length
        expectedBytesLength += _newLinkPathBytes.Length; // NewLinkPath
        expectedBytesLength += 4; // ExistingPath length
        expectedBytesLength += _existingPathBytes.Length; // ExistingPath
        expectedBytesLength += 1; // IsSymLink

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal((uint)bytes.Length - 4, sshDataStream.ReadUInt32());
        Assert.Equal((byte)SftpMessageTypes.Link, sshDataStream.ReadByte());
        Assert.Equal(_requestId, sshDataStream.ReadUInt32());

        Assert.Equal((uint)_newLinkPathBytes.Length, sshDataStream.ReadUInt32());
        var actualNewLinkPath = new byte[_newLinkPathBytes.Length];
        _ = sshDataStream.Read(actualNewLinkPath, 0, actualNewLinkPath.Length);
        Assert.True(_newLinkPathBytes.SequenceEqual(actualNewLinkPath));

        Assert.Equal((uint)_existingPathBytes.Length, sshDataStream.ReadUInt32());
        var actualExistingPath = new byte[_existingPathBytes.Length];
        _ = sshDataStream.Read(actualExistingPath, 0, actualExistingPath.Length);
        Assert.True(_existingPathBytes.SequenceEqual(actualExistingPath));

        Assert.Equal(1, sshDataStream.ReadByte());

        Assert.True(sshDataStream.IsEndOfData);
    }
}
