using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Channels; //was previously: Renci.SshNet.Tests.Classes.Channels;

public class ChannelSessionTest_Disposed_Closed : ChannelSessionTest_Dispose_Disposed
{
    protected override void Act()
    {
        Channel.Dispose();
    }
}
