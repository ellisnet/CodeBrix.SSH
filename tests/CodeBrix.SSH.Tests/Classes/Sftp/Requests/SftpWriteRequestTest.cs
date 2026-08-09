using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Requests;
using CodeBrix.SSH.Sftp.Responses;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp.Requests; //was previously: Renci.SshNet.Tests.Classes.Sftp.Requests;

public class SftpWriteRequestTest
{
    private uint _protocolVersion;
    private uint _requestId;
    private byte[] _handle;
    private ulong _serverFileOffset;
    private byte[] _data;
    private int _offset;
    private int _length;

    public SftpWriteRequestTest()
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
        _serverFileOffset = (ulong)random.Next(0, int.MaxValue);
        _data = new byte[random.Next(10, 15)];
        random.NextBytes(_data);
        _offset = random.Next(0, _data.Length - 1);
        _length = random.Next(0, _data.Length - _offset);
    }

    [Fact]
    public void Constructor()
    {
        var request = new SftpWriteRequest(_protocolVersion, _requestId, _handle, _serverFileOffset, _data, _offset, _length, null);

        Assert.Same(_data, request.Data);
        Assert.Same(_handle, request.Handle);
        Assert.Equal(_length, request.Length);
        Assert.Equal(_offset, request.Offset);
        Assert.Equal(_protocolVersion, request.ProtocolVersion);
        Assert.Equal(_requestId, request.RequestId);
        Assert.Equal(_serverFileOffset, request.ServerFileOffset);
        Assert.Equal(SftpMessageTypes.Write, request.SftpMessageType);
    }

    [Fact]
    public void Complete_SftpStatusResponse()
    {
        var statusActionInvocations = new List<SftpStatusResponse>();
        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        var statusResponse = new SftpStatusResponse(_protocolVersion);

        var request = new SftpWriteRequest(
            _protocolVersion,
            _requestId,
            _handle,
            _serverFileOffset,
            _data,
            _offset,
            _length,
            statusAction);

        request.Complete(statusResponse);

        Assert.Single(statusActionInvocations);
        Assert.Same(statusResponse, statusActionInvocations[0]);
    }

    [Fact]
    public void GetBytes()
    {
        var request = new SftpWriteRequest(_protocolVersion, _requestId, _handle, _serverFileOffset, _data, _offset, _length, null);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 4; // Length
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // RequestId
        expectedBytesLength += 4; // Handle length
        expectedBytesLength += _handle.Length; // Handle
        expectedBytesLength += 8; // ServerFileOffset
        expectedBytesLength += 4; // Data length
        expectedBytesLength += _length; // Data

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal((uint)bytes.Length - 4, sshDataStream.ReadUInt32());
        Assert.Equal((byte)SftpMessageTypes.Write, sshDataStream.ReadByte());
        Assert.Equal(_requestId, sshDataStream.ReadUInt32());

        Assert.Equal((uint)_handle.Length, sshDataStream.ReadUInt32());
        var actualHandle = new byte[_handle.Length];
        _ = sshDataStream.Read(actualHandle, 0, actualHandle.Length);
        Assert.True(_handle.SequenceEqual(actualHandle));

        Assert.Equal(_serverFileOffset, sshDataStream.ReadUInt64());

        Assert.Equal((uint)_length, sshDataStream.ReadUInt32());
        var actualData = new byte[_length];
        _ = sshDataStream.Read(actualData, 0, actualData.Length);
        Assert.True(_data.Take(_offset, _length).SequenceEqual(actualData));

        Assert.True(sshDataStream.IsEndOfData);
    }
}
