using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ForwardedPortRemoteTest_Stop_PortStarted_ChannelBound : ForwardedPortRemoteTest_Dispose_PortStarted_ChannelBound
{
    protected override void Act()
    {
        ForwardedPort.Stop();
    }
}
