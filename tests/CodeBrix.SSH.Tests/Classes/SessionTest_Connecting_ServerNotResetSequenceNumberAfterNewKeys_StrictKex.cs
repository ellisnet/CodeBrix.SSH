using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connecting_ServerNotResetSequenceNumberAfterNewKeys_StrictKex : SessionTest_ConnectingBase
{
    protected override bool ServerSupportsStrictKex
    {
        get
        {
            return true;
        }
    }

    protected override bool ServerResetsSequenceAfterSendingNewKeys
    {
        get
        {
            return false;
        }
    }

    [Fact]
    public void ShouldThrowSshConnectionException()
    {
        var reason = Assert.Throws<SshConnectionException>(Session.Connect).DisconnectReason;
        Assert.Equal(DisconnectReason.MacError, reason);
    }
}
