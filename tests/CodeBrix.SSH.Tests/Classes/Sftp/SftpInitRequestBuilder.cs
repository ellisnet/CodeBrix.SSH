using CodeBrix.SSH.Sftp.Requests;

namespace CodeBrix.SSH.Tests.Classes.Sftp; //was previously: Renci.SshNet.Tests.Classes.Sftp;

internal class SftpInitRequestBuilder
{
    private uint _version;

    public SftpInitRequestBuilder WithVersion(uint version)
    {
        _version = version;
        return this;
    }

    public SftpInitRequest Build()
    {
        return new SftpInitRequest(_version);
    }
}
