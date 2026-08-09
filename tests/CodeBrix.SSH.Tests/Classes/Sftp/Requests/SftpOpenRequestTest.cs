using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Requests;
using CodeBrix.SSH.Sftp.Responses;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp.Requests; //was previously: Renci.SshNet.Tests.Classes.Sftp.Requests;

public class SftpOpenRequestTest
{
    private uint _protocolVersion;
    private uint _requestId;
    private Encoding _encoding;
    private string _filename;
    private byte[] _filenameBytes;
    private Flags _flags;
    private SftpFileAttributes _attributes;
    private byte[] _attributesBytes;

    public SftpOpenRequestTest()
    {
        Init();
    }

    private void Init()
    {
        var random = new Random();

        _protocolVersion = (uint)random.Next(0, int.MaxValue);
        _requestId = (uint)random.Next(0, int.MaxValue);
        _encoding = Encoding.Unicode;
        _filename = random.Next().ToString(CultureInfo.InvariantCulture);
        _filenameBytes = _encoding.GetBytes(_filename);
        _flags = Flags.Read;
        _attributes = SftpFileAttributesBuilder.Empty;
        _attributesBytes = _attributes.GetBytes();
    }

    [Fact]
    public void Constructor()
    {
        var request = new SftpOpenRequest(_protocolVersion, _requestId, _filename, _encoding, _flags, null, null);

        Assert.Same(_encoding, request.Encoding);
        Assert.Equal(_filename, request.Filename);
        Assert.Equal(_protocolVersion, request.ProtocolVersion);
        Assert.Equal(_requestId, request.RequestId);
        Assert.Equal(SftpMessageTypes.Open, request.SftpMessageType);
    }

    [Fact]
    public void Complete_SftpHandleResponse()
    {
        var statusActionInvocations = new List<SftpStatusResponse>();
        var handleActionInvocations = new List<SftpHandleResponse>();

        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        Action<SftpHandleResponse> handleAction = handleActionInvocations.Add;
        var handleResponse = new SftpHandleResponse(_protocolVersion);

        var request = new SftpOpenRequest(
            _protocolVersion,
            _requestId,
            _filename,
            _encoding,
            _flags,
            handleAction,
            statusAction);

        request.Complete(handleResponse);

        Assert.Empty(statusActionInvocations);
        Assert.Single(handleActionInvocations);
        Assert.Same(handleResponse, handleActionInvocations[0]);
    }

    [Fact]
    public void Complete_SftpStatusResponse()
    {
        var statusActionInvocations = new List<SftpStatusResponse>();
        var handleActionInvocations = new List<SftpHandleResponse>();

        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        Action<SftpHandleResponse> handleAction = handleActionInvocations.Add;
        var statusResponse = new SftpStatusResponse(_protocolVersion);

        var request = new SftpOpenRequest(
            _protocolVersion,
            _requestId,
            _filename,
            _encoding,
            _flags,
            handleAction,
            statusAction);

        request.Complete(statusResponse);

        Assert.Single(statusActionInvocations);
        Assert.Same(statusResponse, statusActionInvocations[0]);
        Assert.Empty(handleActionInvocations);
    }

    [Fact]
    public void GetBytes()
    {
        var request = new SftpOpenRequest(_protocolVersion, _requestId, _filename, _encoding, _flags, null, null);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 4; // Length
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // RequestId
        expectedBytesLength += 4; // Filename length
        expectedBytesLength += _filenameBytes.Length; // Filename
        expectedBytesLength += 4; // Flags
        expectedBytesLength += _attributesBytes.Length; // Attributes

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal((uint)bytes.Length - 4, sshDataStream.ReadUInt32());
        Assert.Equal((byte)SftpMessageTypes.Open, sshDataStream.ReadByte());
        Assert.Equal(_requestId, sshDataStream.ReadUInt32());

        Assert.Equal((uint)_filenameBytes.Length, sshDataStream.ReadUInt32());
        var actualPath = new byte[_filenameBytes.Length];
        _ = sshDataStream.Read(actualPath, 0, actualPath.Length);
        Assert.True(_filenameBytes.SequenceEqual(actualPath));

        Assert.Equal((uint)_flags, sshDataStream.ReadUInt32());

        var actualAttributes = new byte[_attributesBytes.Length];
        _ = sshDataStream.Read(actualAttributes, 0, actualAttributes.Length);
        Assert.True(_attributesBytes.SequenceEqual(actualAttributes));

        Assert.True(sshDataStream.IsEndOfData);
    }
}
