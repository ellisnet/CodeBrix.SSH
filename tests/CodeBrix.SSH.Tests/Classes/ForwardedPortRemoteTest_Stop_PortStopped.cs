using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortRemoteTest_Stop_PortStopped : ForwardedPortRemoteTest_Dispose_PortStopped
{
    protected override void Act()
    {
        ForwardedPort.Stop();
    }
}
