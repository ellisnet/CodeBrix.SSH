using System.Net.Sockets;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connecting_ServerSendsIgnoreMessageBeforeKexInit_StrictKex : SessionTest_ConnectingBase
{
    protected override bool ServerSupportsStrictKex
    {
        get
        {
            return true;
        }
    }

    protected override void ActionBeforeKexInit()
    {
        var ignoreMessage = new IgnoreMessage();
        var ignore = ignoreMessage.GetPacket(8, null);

        // MitM sends ignore message to client
        _ = ServerSocket.Send(ignore, 4, ignore.Length - 4, SocketFlags.None);

        // MitM drops server message
        ServerOutboundPacketSequence++;
    }

    [Fact]
    public void ShouldThrowSshConnectionException()
    {
        var exception = Assert.Throws<SshConnectionException>(Session.Connect);
        Assert.Equal(DisconnectReason.KeyExchangeFailed, exception.DisconnectReason);
        Assert.Equal("KEXINIT was not the first packet during strict key exchange.", exception.Message);
    }
}
