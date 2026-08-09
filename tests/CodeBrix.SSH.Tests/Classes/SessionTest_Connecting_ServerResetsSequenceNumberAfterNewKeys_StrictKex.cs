using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connecting_ServerResetsSequenceNumberAfterNewKeys_StrictKex : SessionTest_ConnectingBase
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
            return true;
        }
    }

    [Fact]
    public void ShouldNotThrowException()
    {
        Session.Connect();
    }
}
