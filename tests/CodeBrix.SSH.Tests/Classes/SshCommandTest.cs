using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.SSH.Tests.Properties;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Represents SSH command that can be executed.
/// </summary>
public partial class SshCommandTest : TestBase
{
    [Fact]
    public void Test_Execute_SingleCommand_Without_Connecting()
    {
        using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
        {
            Assert.Throws<SshConnectionException>(() => client.CreateCommand("echo Hello"));
        }
    }
}
