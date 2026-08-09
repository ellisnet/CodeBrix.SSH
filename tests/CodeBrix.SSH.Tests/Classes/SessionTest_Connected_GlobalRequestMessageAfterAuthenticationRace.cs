using SilverAssertions;
using System.Net.Sockets;
using System.Text;
using CodeBrix.SSH.Messages.Connection;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

/// <summary>
/// Test for https://github.com/sshnet/CodeBrix.SSH/issues/8.
/// </summary>
public class SessionTest_Connected_GlobalRequestMessageAfterAuthenticationRace : SessionTest_ConnectedBase
{
    private GlobalRequestMessage _globalRequestMessage;

    protected override void SetupData()
    {
        base.SetupData();

        _globalRequestMessage = new GlobalRequestMessage(Encoding.ASCII.GetBytes("ping-mocana-com"), false);
    }

    protected override void Act()
    {
    }

    protected override void ClientAuthentication_Callback()
    {
        var globalRequest = _globalRequestMessage.GetPacket(8, null);
        ServerSocket.Send(globalRequest, 4, globalRequest.Length - 4, SocketFlags.None);
    }

    [Fact]
    public void ErrorOccurredShouldNotBeRaised()
    {
        ErrorOccurredRegister.Count.Should().Be(0, ErrorOccurredRegister.AsString());
    }
}
