using CodeBrix.SSH.Connection;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connecting_ServerIdentificationReceived : SessionTest_ConnectingBase
{
    protected override void SetupData()
    {
        base.SetupData();

        Session.ServerIdentificationReceived += (s, e) =>
        {
            if ((e.SshIdentification.SoftwareVersion.StartsWith("OpenSSH_6.5", System.StringComparison.Ordinal) || e.SshIdentification.SoftwareVersion.StartsWith("OpenSSH_6.6", System.StringComparison.Ordinal))
                   && !e.SshIdentification.SoftwareVersion.StartsWith("OpenSSH_6.6.1", System.StringComparison.Ordinal))
            {
                _ = ConnectionInfo.KeyExchangeAlgorithms.Remove("curve25519-sha256");
                _ = ConnectionInfo.KeyExchangeAlgorithms.Remove("curve25519-sha256@libssh.org");
            }
        };
    }

    [Theory]
    [InlineData("OpenSSH_6.5")]
    [InlineData("OpenSSH_6.5p1")]
    [InlineData("OpenSSH_6.5 PKIX")]
    [InlineData("OpenSSH_6.6")]
    [InlineData("OpenSSH_6.6p1")]
    [InlineData("OpenSSH_6.6 PKIX")]
    public void ShouldExcludeCurve25519KexWhenServerIs(string softwareVersion)
    {
        ServerIdentification = new SshIdentification("2.0", softwareVersion);

        Session.Connect();

        Assert.False(ConnectionInfo.KeyExchangeAlgorithms.ContainsKey("curve25519-sha256"));
        Assert.False(ConnectionInfo.KeyExchangeAlgorithms.ContainsKey("curve25519-sha256@libssh.org"));
    }

    [Theory]
    [InlineData("OpenSSH_6.6.1")]
    [InlineData("OpenSSH_6.6.1p1")]
    [InlineData("OpenSSH_6.6.1 PKIX")]
    [InlineData("OpenSSH_6.7")]
    [InlineData("OpenSSH_6.7p1")]
    [InlineData("OpenSSH_6.7 PKIX")]
    public void ShouldIncludeCurve25519KexWhenServerIs(string softwareVersion)
    {
        ServerIdentification = new SshIdentification("2.0", softwareVersion);

        Session.Connect();

        Assert.True(ConnectionInfo.KeyExchangeAlgorithms.ContainsKey("curve25519-sha256"));
        Assert.True(ConnectionInfo.KeyExchangeAlgorithms.ContainsKey("curve25519-sha256@libssh.org"));
    }
}
