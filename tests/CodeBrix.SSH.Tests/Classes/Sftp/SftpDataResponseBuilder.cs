using CodeBrix.SSH.Sftp.Responses;

namespace CodeBrix.SSH.Tests.Classes.Sftp; //was previously: Renci.SshNet.Tests.Classes.Sftp;

internal class SftpDataResponseBuilder
{
    private uint _protocolVersion;
    private uint _responseId;
    private byte[] _data;

    public SftpDataResponseBuilder WithProtocolVersion(uint protocolVersion)
    {
        _protocolVersion = protocolVersion;
        return this;
    }

    public SftpDataResponseBuilder WithResponseId(uint responseId)
    {
        _responseId = responseId;
        return this;
    }

    public SftpDataResponseBuilder WithData(byte[] data)
    {
        _data = data;
        return this;
    }

    public SftpDataResponse Build()
    {
        return new SftpDataResponse(_protocolVersion)
        {
            ResponseId = _responseId,
            Data = _data
        };
    }
}
