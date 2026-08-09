using System.Globalization;
using System.Net.Sockets;
using System.Text;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Transport;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SessionTest_Connecting_ServerSendsDebugMessageAfterKexInit_StrictKex : SessionTest_ConnectingBase
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
        using var stream = new SshDataStream(0);
        stream.WriteByte(1);
        stream.Write("This is a debug message", Encoding.UTF8);
        stream.Write(CultureInfo.CurrentCulture.Name, Encoding.UTF8);

        var debugMessage = new DebugMessage();
        debugMessage.Load(stream.ToArray());
        var debug = debugMessage.GetPacket(8, null);

        // MitM sends debug message to client
        _ = ServerSocket.Send(debug, 4, debug.Length - 4, SocketFlags.None);

        // MitM drops server message
        ServerOutboundPacketSequence++;
    }

    [Fact]
    public void ShouldThrowSshException()
    {
        var message = Assert.Throws<SshException>(Session.Connect).Message;
        Assert.Equal("Message type 4 is not valid in the current context.", message);
    }
}
