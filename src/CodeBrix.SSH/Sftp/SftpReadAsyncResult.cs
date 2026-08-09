using System;
using CodeBrix.SSH.Common;

namespace CodeBrix.SSH.Sftp; //was previously: Renci.SshNet.Sftp;

internal sealed class SftpReadAsyncResult : AsyncResult<byte[]>
{
    public SftpReadAsyncResult(AsyncCallback asyncCallback, object state)
        : base(asyncCallback, state)
    {
    }
}
