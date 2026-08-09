using System.Collections.Generic;
using CodeBrix.SSH.Sftp.Responses;

namespace CodeBrix.SSH.Tests.Classes.Sftp; //was previously: Renci.SshNet.Tests.Classes.Sftp;

internal class SftpVersionResponseBuilder
{
    private uint _version;
    private readonly IDictionary<string, string> _extensions;

    public SftpVersionResponseBuilder()
    {
        _extensions = new Dictionary<string, string>();
    }

    public SftpVersionResponseBuilder WithVersion(uint version)
    {
        _version = version;
        return this;
    }

    public SftpVersionResponseBuilder WithExtension(string name, string data)
    {
        _extensions.Add(name, data);
        return this;
    }

    public SftpVersionResponse Build()
    {
        var sftpVersionResponse = new SftpVersionResponse()
        {
            Version = _version,
            Extensions = _extensions
        };
        return sftpVersionResponse;
    }
}
