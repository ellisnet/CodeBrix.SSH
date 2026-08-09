using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Connection;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Connection; //was previously: Renci.SshNet.Tests.Classes.Connection;

public class ProtocolVersionExchangeTest_ServerResponseInvalid_SshIdentificationOnlyContainsProtocolVersion : IDisposable
{
    private AsyncSocketListener _server;
    private ProtocolVersionExchange _protocolVersionExchange;
    private string _clientVersion;
    private TimeSpan _timeout;
    private IPEndPoint _serverEndPoint;
    private List<byte> _dataReceivedByServer;
    private byte[] _serverIdentification;
    private bool _clientDisconnected;
    private Socket _client;
    private SshConnectionException _actualException;

    public ProtocolVersionExchangeTest_ServerResponseInvalid_SshIdentificationOnlyContainsProtocolVersion()
    {
        Setup();
    }

    private void Setup()
    {
        Arrange();
        Act();
    }

    public void Dispose()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        if (_server != null)
        {
            _server.Dispose();
            _server = null;
        }

        if (_client != null)
        {
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
            _client = null;
        }
    }

    protected void Arrange()
    {
        _clientVersion = "SSH-2.0-CodeBrix.SSH.SshClient.0.0.1";
        _timeout = TimeSpan.FromSeconds(5);
        _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
        _dataReceivedByServer = new List<byte>();
        _serverIdentification = Encoding.UTF8.GetBytes("SSH-2.0\r\n");

        _server = new AsyncSocketListener(_serverEndPoint);
        _server.Start();
        _server.BytesReceived += (bytes, socket) =>
            {
                _dataReceivedByServer.AddRange(bytes);

                _ = socket.Send(_serverIdentification);

                socket.Shutdown(SocketShutdown.Send);
            };
        _server.Disconnected += (socket) => _clientDisconnected = true;

        _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _client.Connect(_serverEndPoint);

        _protocolVersionExchange = new ProtocolVersionExchange();
    }

    protected void Act()
    {
        try
        {
            _ = _protocolVersionExchange.Start(_clientVersion, _client, _timeout);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshConnectionException ex)
        {
            _actualException = ex;
        }

        // Give some time to process all messages
        Thread.Sleep(200);
    }

    [Fact]
    public void StartShouldHaveThrownSshConnectionException()
    {
        var expectedMessage = string.Format("The server response does not contain an SSH identification string:{0}{0}" +
                                            "  00000000  53 53 48 2D 32 2E 30 0D 0A                       SSH-2.0..{0}{0}" +
                                            "More information on the Protocol Version Exchange is available here:{0}" +
                                            "https://tools.ietf.org/html/rfc4253#section-4.2",
                                            Environment.NewLine);

        Assert.NotNull(_actualException);
        Assert.Null(_actualException.InnerException);
        Assert.Equal(expectedMessage, _actualException.Message);
    }

    [Fact]
    public void ClientIdentificationWasSentToServer()
    {
        var expected = Encoding.UTF8.GetBytes(_clientVersion);

        Assert.Equal(expected.Length + 2, _dataReceivedByServer.Count);

        Assert.True(expected.SequenceEqual(_dataReceivedByServer.Take(expected.Length)));
        Assert.Equal(Session.CarriageReturn, _dataReceivedByServer[_dataReceivedByServer.Count - 2]);
        Assert.Equal(Session.LineFeed, _dataReceivedByServer[_dataReceivedByServer.Count - 1]);
    }

    [Fact]
    public void ConnectionIsClosedByServer()
    {
        Assert.True(_client.Connected);
        Assert.False(_clientDisconnected);

        var bytesReceived = _client.Receive(new byte[1]);
        Assert.Equal(0, bytesReceived);

        Assert.True(_client.Connected);
        Assert.False(_clientDisconnected);
    }
}
