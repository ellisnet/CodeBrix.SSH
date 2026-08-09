using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Requests;
using CodeBrix.SSH.Sftp.Responses;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp.Requests; //was previously: Renci.SshNet.Tests.Classes.Sftp.Requests;

public class SftpBlockRequestTest
{
    private uint _protocolVersion;
    private uint _requestId;
    private byte[] _handle;
    private ulong _offset;
    private ulong _length;
    private uint _lockMask;

    public SftpBlockRequestTest()
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
        _length = (ulong)random.Next(0, int.MaxValue);
        _lockMask = (uint)random.Next(0, int.MaxValue);
    }

    [Fact]
    public void Constructor()
    {
        var request = new SftpBlockRequest(_protocolVersion, _requestId, _handle, _offset, _length, _lockMask, null);

        Assert.Same(_handle, request.Handle);
        Assert.Equal(_length, request.Length);
        Assert.Equal(_lockMask, request.LockMask);
        Assert.Equal(_offset, request.Offset);
        Assert.Equal(_protocolVersion, request.ProtocolVersion);
        Assert.Equal(_requestId, request.RequestId);
        Assert.Equal(SftpMessageTypes.Block, request.SftpMessageType);
    }

    [Fact]
    public void Complete_SftpStatusResponse()
    {
        IList<SftpStatusResponse> statusActionInvocations = new List<SftpStatusResponse>();

        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        var statusResponse = new SftpStatusResponse(_protocolVersion);

        var request = new SftpBlockRequest(_protocolVersion, _requestId, _handle, _offset, _length, _lockMask, statusAction);

        request.Complete(statusResponse);

        Assert.Single(statusActionInvocations);
        Assert.Same(statusResponse, statusActionInvocations[0]);
    }

    [Fact]
    public void GetBytes()
    {
        var request = new SftpBlockRequest(_protocolVersion, _requestId, _handle, _offset, _length, _lockMask, null);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 4; // Length
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // RequestId
        expectedBytesLength += 4; // Handle length
        expectedBytesLength += _handle.Length; // Handle
        expectedBytesLength += 8; // Offset
        expectedBytesLength += 8; // Length
        expectedBytesLength += 4; // LockMask

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal((uint)bytes.Length - 4, sshDataStream.ReadUInt32());
        Assert.Equal((byte)SftpMessageTypes.Block, sshDataStream.ReadByte());
        Assert.Equal(_requestId, sshDataStream.ReadUInt32());

        Assert.Equal((uint)_handle.Length, sshDataStream.ReadUInt32());
        var actualHandle = new byte[_handle.Length];
        _ = sshDataStream.Read(actualHandle, 0, actualHandle.Length);
        Assert.True(_handle.SequenceEqual(actualHandle));

        Assert.Equal(_offset, sshDataStream.ReadUInt64());
        Assert.Equal(_length, sshDataStream.ReadUInt64());
        Assert.Equal(_lockMask, sshDataStream.ReadUInt32());
        Assert.True(sshDataStream.IsEndOfData);
    }
}
