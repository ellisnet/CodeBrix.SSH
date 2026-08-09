using System.Net.Sockets;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connecting_ServerSendsIgnoreMessageAfterKexInit_StrictKex : SessionTest_ConnectingBase
{
    protected override bool ServerSupportsStrictKex
    {
        get
        {
            return true;
        }
    }

    protected override void ActionAfterKexInit()
    {
        var ignoreMessage = new IgnoreMessage();
        var ignore = ignoreMessage.GetPacket(8, null);

        // MitM sends ignore message to client
        _ = ServerSocket.Send(ignore, 4, ignore.Length - 4, SocketFlags.None);

        // MitM drops server message
        ServerOutboundPacketSequence++;
    }

    [Fact]
    public void ShouldThrowSshException()
    {
        var message = Assert.Throws<SshException>(Session.Connect).Message;
        Assert.Equal("Message type 2 is not valid in the current context.", message);
    }
}
