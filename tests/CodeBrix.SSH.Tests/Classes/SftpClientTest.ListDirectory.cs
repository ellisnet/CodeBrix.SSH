using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Properties;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
/// </summary>
public partial class SftpClientTest
{
    [Fact]
    [Trait("Category", "Sftp")]
    public void Test_Sftp_ListDirectory_Without_Connecting()
    {
        using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
        {
            Assert.Throws<SshConnectionException>(() => sftp.ListDirectory("."));
        }
    }
}
