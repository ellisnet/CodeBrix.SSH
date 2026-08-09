using System;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Properties;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public partial class SftpClientTest
{
    [Fact]
    public void GetAttributes_Throws_WhenNotConnected()
    {
        using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
        {
            Assert.Throws<SshConnectionException>(() => sftp.GetAttributes("."));
        }
    }

    [Fact]
    public void GetAttributes_Throws_WhenDisposed()
    {
        var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD);
        sftp.Dispose();

        Assert.Throws<ObjectDisposedException>(() => sftp.GetAttributes("."));
    }
}
