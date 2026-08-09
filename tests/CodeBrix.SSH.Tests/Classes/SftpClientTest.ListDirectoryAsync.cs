using System.Threading;
using System.Threading.Tasks;
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
    public async Task Test_Sftp_ListDirectoryAsync_Without_ConnectingAsync()
    {
        using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
        {
            await Assert.ThrowsAsync<SshConnectionException>(async () =>
            {
                await foreach (var x in sftp.ListDirectoryAsync(".", CancellationToken.None))
                {
                }
            });
        }
    }
}
