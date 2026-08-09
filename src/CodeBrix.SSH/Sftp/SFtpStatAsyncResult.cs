using System;
using CodeBrix.SSH.Common;

namespace CodeBrix.SSH.Sftp; //was previously: Renci.SshNet.Sftp;

internal sealed class SFtpStatAsyncResult : AsyncResult<SftpFileAttributes>
{
    public SFtpStatAsyncResult(AsyncCallback asyncCallback, object state)
        : base(asyncCallback, state)
    {
    }
}
