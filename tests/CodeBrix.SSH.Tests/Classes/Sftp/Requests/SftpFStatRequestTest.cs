using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Requests;
using CodeBrix.SSH.Sftp.Responses;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp.Requests; //was previously: Renci.SshNet.Tests.Classes.Sftp.Requests;

public class SftpFStatRequestTest
{
    private uint _protocolVersion;
    private uint _requestId;
    private byte[] _handle;

    public SftpFStatRequestTest()
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
    }

    [Fact]
    public void Constructor()
    {
        var request = new SftpFStatRequest(_protocolVersion, _requestId, _handle, null, null);

        Assert.Same(_handle, request.Handle);
        Assert.Equal(_protocolVersion, request.ProtocolVersion);
        Assert.Equal(_requestId, request.RequestId);
        Assert.Equal(SftpMessageTypes.FStat, request.SftpMessageType);
    }

    [Fact]
    public void Complete_SftpAttrsResponse()
    {
        var statusActionInvocations = new List<SftpStatusResponse>();
        var attrsActionInvocations = new List<SftpAttrsResponse>();

        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        Action<SftpAttrsResponse> attrsAction = attrsActionInvocations.Add;
        var attrsResponse = new SftpAttrsResponse(_protocolVersion);

        var request = new SftpFStatRequest(_protocolVersion, _requestId, _handle, attrsAction, statusAction);

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

        var request = new SftpFStatRequest(_protocolVersion, _requestId, _handle, attrsAction, statusAction);

        request.Complete(statusResponse);

        Assert.Single(statusActionInvocations);
        Assert.Same(statusResponse, statusActionInvocations[0]);
        Assert.Empty(attrsActionInvocations);
    }

    [Fact]
    public void GetBytes()
    {
        var request = new SftpFStatRequest(_protocolVersion, _requestId, _handle, null, null);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 4; // Length
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // RequestId
        expectedBytesLength += 4; // Handle length
        expectedBytesLength += _handle.Length; // Handle

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal((uint)bytes.Length - 4, sshDataStream.ReadUInt32());
        Assert.Equal((byte)SftpMessageTypes.FStat, sshDataStream.ReadByte());
        Assert.Equal(_requestId, sshDataStream.ReadUInt32());

        Assert.Equal((uint)_handle.Length, sshDataStream.ReadUInt32());
        var actualHandle = new byte[_handle.Length];
        _ = sshDataStream.Read(actualHandle, 0, actualHandle.Length);
        Assert.True(_handle.SequenceEqual(actualHandle));

        Assert.True(sshDataStream.IsEndOfData);
    }
}
