using System.Net.Sockets;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connecting_ServerSendsDisconnectMessageAfterKexInit_StrictKex : SessionTest_ConnectingBase
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
        var disconnectMessage = new DisconnectMessage(DisconnectReason.TooManyConnections, "too many connections");
        var disconnect = disconnectMessage.GetPacket(8, null);

        // Server sends disconnect message to client
        _ = ServerSocket.Send(disconnect, 4, disconnect.Length - 4, SocketFlags.None);

        ServerOutboundPacketSequence++;
    }

    [Fact]
    public void DisconnectIsAllowedDuringStrictKex()
    {
        var exception = Assert.Throws<SshConnectionException>(Session.Connect);
        Assert.Equal(DisconnectReason.TooManyConnections, exception.DisconnectReason);
    }
}
