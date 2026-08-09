using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Requests;
using CodeBrix.SSH.Sftp.Responses;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp.Requests; //was previously: Renci.SshNet.Tests.Classes.Sftp.Requests;

public class SftpReadRequestTest
{
    private uint _protocolVersion;
    private uint _requestId;
    private byte[] _handle;
    private ulong _offset;
    private uint _length;

    public SftpReadRequestTest()
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
        _offset = (ulong)random.Next(0, int.MaxValue);
        _length = (uint)random.Next(0, int.MaxValue);
    }

    [Fact]
    public void Constructor()
    {
        var request = new SftpReadRequest(_protocolVersion, _requestId, _handle, _offset, _length, null, null);

        Assert.Same(_handle, request.Handle);
        Assert.Equal(_length, request.Length);
        Assert.Equal(_offset, request.Offset);
        Assert.Equal(_protocolVersion, request.ProtocolVersion);
        Assert.Equal(_requestId, request.RequestId);
        Assert.Equal(SftpMessageTypes.Read, request.SftpMessageType);
    }

    [Fact]
    public void Complete_SftpDataResponse()
    {
        var statusActionInvocations = new List<SftpStatusResponse>();
        var dataActionInvocations = new List<SftpDataResponse>();

        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        Action<SftpDataResponse> dataAction = dataActionInvocations.Add;
        var dataResponse = new SftpDataResponse(_protocolVersion);

        var request = new SftpReadRequest(
            _protocolVersion,
            _requestId,
            _handle,
            _offset,
            _length,
            dataAction,
            statusAction);

        request.Complete(dataResponse);

        Assert.Empty(statusActionInvocations);
        Assert.Single(dataActionInvocations);
        Assert.Same(dataResponse, dataActionInvocations[0]);
    }

    [Fact]
    public void Complete_SftpStatusResponse()
    {
        var statusActionInvocations = new List<SftpStatusResponse>();
        var dataActionInvocations = new List<SftpDataResponse>();

        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        Action<SftpDataResponse> dataAction = dataActionInvocations.Add;
        var statusResponse = new SftpStatusResponse(_protocolVersion);

        var request = new SftpReadRequest(
            _protocolVersion,
            _requestId,
            _handle,
            _offset,
            _length,
            dataAction,
            statusAction);

        request.Complete(statusResponse);

        Assert.Single(statusActionInvocations);
        Assert.Same(statusResponse, statusActionInvocations[0]);
        Assert.Empty(dataActionInvocations);
    }

    [Fact]
    public void GetBytes()
    {
        var request = new SftpReadRequest(_protocolVersion, _requestId, _handle, _offset, _length, null, null);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 4; // Length
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // RequestId
        expectedBytesLength += 4; // Handle length
        expectedBytesLength += _handle.Length; // Handle
        expectedBytesLength += 8; // Offset
        expectedBytesLength += 4; // Length

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal((uint)bytes.Length - 4, sshDataStream.ReadUInt32());
        Assert.Equal((byte)SftpMessageTypes.Read, sshDataStream.ReadByte());
        Assert.Equal(_requestId, sshDataStream.ReadUInt32());

        Assert.Equal((uint)_handle.Length, sshDataStream.ReadUInt32());
        var actualHandle = new byte[_handle.Length];
        _ = sshDataStream.Read(actualHandle, 0, actualHandle.Length);
        Assert.True(_handle.SequenceEqual(actualHandle));

        Assert.Equal(_offset, sshDataStream.ReadUInt64());
        Assert.Equal(_length, sshDataStream.ReadUInt32());

        Assert.True(sshDataStream.IsEndOfData);
    }
}
