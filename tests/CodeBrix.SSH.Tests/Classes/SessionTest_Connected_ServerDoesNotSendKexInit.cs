using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connected_ServerDoesNotSendKexInit : SessionTest_ConnectedBase
{
    protected override void SetupData()
    {
        WaitForClientKeyExchangeInit = true;

        base.SetupData();
    }

    protected override void Act()
    {
    }

    [Fact]
    public void ConnectShouldSucceed()
    {
    }
}
