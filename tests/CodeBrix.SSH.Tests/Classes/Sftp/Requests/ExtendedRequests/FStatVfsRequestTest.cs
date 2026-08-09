using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Requests;
using CodeBrix.SSH.Sftp.Responses;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp.Requests.ExtendedRequests; //was previously: Renci.SshNet.Tests.Classes.Sftp.Requests.ExtendedRequests;

public class FStatVfsRequestTest
{
    private uint _protocolVersion;
    private uint _requestId;
    private byte[] _handle;
    private string _name;
    private byte[] _nameBytes;

    public FStatVfsRequestTest()
    {
        Init();
    }

    private void Init()
    {
        var random = new Random();
        _protocolVersion = (uint)random.Next(0, int.MaxValue);
        _requestId = (uint)random.Next(0, int.MaxValue);
        _handle = new byte[random.Next(1, 10)];
        random.NextBytes(_handle);

        _name = "fstatvfs@openssh.com";
        _nameBytes = Encoding.UTF8.GetBytes(_name);
    }

    [Fact]
    public void Constructor()
    {
        var request = new FStatVfsRequest(_protocolVersion, _requestId, _handle, null, null);

        Assert.Same(_handle, request.Handle);
        Assert.Equal(_name, request.Name);
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

        var request = new FStatVfsRequest(_protocolVersion, _requestId, _handle, extendedAction, statusAction);

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

        var request = new FStatVfsRequest(_protocolVersion, _requestId, _handle, extendedAction, statusAction);

        request.Complete(extendedReplyResponse);

        Assert.Empty(statusActionInvocations);
        Assert.Single(extendedReplyActionInvocations);
        Assert.Same(extendedReplyResponse, extendedReplyActionInvocations[0]);
    }

    [Fact]
    public void GetBytes()
    {
        var request = new FStatVfsRequest(_protocolVersion, _requestId, _handle, null, null);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 4; // Length
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // RequestId
        expectedBytesLength += 4; // Name length
        expectedBytesLength += _nameBytes.Length; // Name
        expectedBytesLength += 4; // Handle length
        expectedBytesLength += _handle.Length; // Handle

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal((uint)bytes.Length - 4, sshDataStream.ReadUInt32());
        Assert.Equal((byte)SftpMessageTypes.Extended, sshDataStream.ReadByte());
        Assert.Equal(_requestId, sshDataStream.ReadUInt32());
        Assert.Equal((uint)_nameBytes.Length, sshDataStream.ReadUInt32());

        var actualNameBytes = new byte[_nameBytes.Length];
        _ = sshDataStream.Read(actualNameBytes, 0, actualNameBytes.Length);
        Assert.True(_nameBytes.SequenceEqual(actualNameBytes));

        Assert.Equal((uint)_handle.Length, sshDataStream.ReadUInt32());

        var actualHandle = new byte[_handle.Length];
        _ = sshDataStream.Read(actualHandle, 0, actualHandle.Length);
        Assert.True(_handle.SequenceEqual(actualHandle));

        Assert.True(sshDataStream.IsEndOfData);
    }
}
