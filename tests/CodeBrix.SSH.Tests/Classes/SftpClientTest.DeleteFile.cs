using System;
using CodeBrix.SSH.Tests.Properties;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
/// </summary>
public partial class SftpClientTest
{
    [Fact]
    public void Test_Sftp_DeleteFile_Null()
    {
        using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
        {
            Assert.Throws<ArgumentNullException>(() => sftp.DeleteFile(null));
        }
    }
}
