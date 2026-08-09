using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Requests;
using CodeBrix.SSH.Sftp.Responses;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp.Requests; //was previously: Renci.SshNet.Tests.Classes.Sftp.Requests;

public class SftpFSetStatRequestTest
{
    private uint _protocolVersion;
    private uint _requestId;
    private byte[] _handle;
    private SftpFileAttributes _attributes;
    private byte[] _attributesBytes;

    public SftpFSetStatRequestTest()
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
        _attributes = SftpFileAttributesBuilder.Empty;
        _attributesBytes = _attributes.GetBytes();
    }

    [Fact]
    public void Constructor()
    {
        var request = new SftpFSetStatRequest(_protocolVersion, _requestId, _handle, _attributes, null);

        Assert.Same(_handle, request.Handle);
        Assert.Equal(_protocolVersion, request.ProtocolVersion);
        Assert.Equal(_requestId, request.RequestId);
        Assert.Equal(SftpMessageTypes.FSetStat, request.SftpMessageType);
    }

    [Fact]
    public void Complete_SftpStatusResponse()
    {
        IList<SftpStatusResponse> statusActionInvocations = new List<SftpStatusResponse>();

        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        var statusResponse = new SftpStatusResponse(_protocolVersion);

        var request = new SftpFSetStatRequest(_protocolVersion, _requestId, _handle, _attributes, statusAction);

        request.Complete(statusResponse);

        Assert.Single(statusActionInvocations);
        Assert.Same(statusResponse, statusActionInvocations[0]);
    }

    [Fact]
    public void GetBytes()
    {
        var request = new SftpFSetStatRequest(_protocolVersion, _requestId, _handle, _attributes, null);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 4; // Length
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // RequestId
        expectedBytesLength += 4; // Handle length
        expectedBytesLength += _handle.Length; // Handle
        expectedBytesLength += _attributesBytes.Length; // Attributes

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal((uint)bytes.Length - 4, sshDataStream.ReadUInt32());
        Assert.Equal((byte)SftpMessageTypes.FSetStat, sshDataStream.ReadByte());
        Assert.Equal(_requestId, sshDataStream.ReadUInt32());

        Assert.Equal((uint)_handle.Length, sshDataStream.ReadUInt32());
        var actualHandle = new byte[_handle.Length];
        _ = sshDataStream.Read(actualHandle, 0, actualHandle.Length);
        Assert.True(_handle.SequenceEqual(actualHandle));

        var actualAttributes = new byte[_attributesBytes.Length];
        _ = sshDataStream.Read(actualAttributes, 0, actualAttributes.Length);
        Assert.True(_attributesBytes.SequenceEqual(actualAttributes));

        Assert.True(sshDataStream.IsEndOfData);
    }
}
