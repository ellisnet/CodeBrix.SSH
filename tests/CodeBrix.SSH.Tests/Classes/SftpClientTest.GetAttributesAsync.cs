using System;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Properties;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public partial class SftpClientTest
{
    [Fact]
    public async Task GetAttributesAsync_Throws_WhenNotConnected()
    {
        using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
        {
            await Assert.ThrowsAsync<SshConnectionException>(() => sftp.GetAttributesAsync(".", CancellationToken.None));
        }
    }

    [Fact]
    public async Task GetAttributesAsync_Throws_WhenDisposed()
    {
        var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD);
        sftp.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => sftp.GetAttributesAsync(".", CancellationToken.None));
    }
}
