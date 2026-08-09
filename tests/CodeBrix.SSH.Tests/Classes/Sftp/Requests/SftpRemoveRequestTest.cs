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

public class SftpRemoveRequestTest
{
    private uint _protocolVersion;
    private uint _requestId;
    private Encoding _encoding;
    private string _filename;
    private byte[] _filenameBytes;

    public SftpRemoveRequestTest()
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
    }

    [Fact]
    public void Constructor()
    {
        var request = new SftpRemoveRequest(_protocolVersion, _requestId, _filename, _encoding, null);

        Assert.Same(_encoding, request.Encoding);
        Assert.Equal(_filename, request.Filename);
        Assert.Equal(_protocolVersion, request.ProtocolVersion);
        Assert.Equal(_requestId, request.RequestId);
        Assert.Equal(SftpMessageTypes.Remove, request.SftpMessageType);
    }

    [Fact]
    public void Complete_SftpStatusResponse()
    {
        var statusActionInvocations = new List<SftpStatusResponse>();

        Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
        var statusResponse = new SftpStatusResponse(_protocolVersion);

        var request = new SftpRemoveRequest(_protocolVersion, _requestId, _filename, _encoding, statusAction);

        request.Complete(statusResponse);

        Assert.Single(statusActionInvocations);
        Assert.Same(statusResponse, statusActionInvocations[0]);
    }

    [Fact]
    public void GetBytes()
    {
        var request = new SftpRemoveRequest(_protocolVersion, _requestId, _filename, _encoding, null);

        var bytes = request.GetBytes();

        var expectedBytesLength = 0;
        expectedBytesLength += 4; // Length
        expectedBytesLength += 1; // Type
        expectedBytesLength += 4; // RequestId
        expectedBytesLength += 4; // Filename length
        expectedBytesLength += _filenameBytes.Length; // Filename

        Assert.Equal(expectedBytesLength, bytes.Length);

        var sshDataStream = new SshDataStream(bytes);

        Assert.Equal((uint)bytes.Length - 4, sshDataStream.ReadUInt32());
        Assert.Equal((byte)SftpMessageTypes.Remove, sshDataStream.ReadByte());
        Assert.Equal(_requestId, sshDataStream.ReadUInt32());

        Assert.Equal((uint)_filenameBytes.Length, sshDataStream.ReadUInt32());
        var actualFilename = new byte[_filenameBytes.Length];
        _ = sshDataStream.Read(actualFilename, 0, actualFilename.Length);
        Assert.True(_filenameBytes.SequenceEqual(actualFilename));

        Assert.True(sshDataStream.IsEndOfData);
    }
}
