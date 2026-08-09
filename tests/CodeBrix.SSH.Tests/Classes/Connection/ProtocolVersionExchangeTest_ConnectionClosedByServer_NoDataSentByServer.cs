using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Connection;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Connection; //was previously: Renci.SshNet.Tests.Classes.Connection;

public class ProtocolVersionExchangeTest_ConnectionClosedByServer_NoDataSentByServer : IDisposable
{
    private AsyncSocketListener _server;
    private ProtocolVersionExchange _protocolVersionExchange;
    private string _clientVersion;
    private TimeSpan _timeout;
    private IPEndPoint _serverEndPoint;
    private List<byte> _dataReceivedByServer;
    private bool _clientDisconnected;
    private Socket _client;
    private SshConnectionException _actualException;

    public ProtocolVersionExchangeTest_ConnectionClosedByServer_NoDataSentByServer()
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
        _clientVersion = "\uD55C";
        _timeout = TimeSpan.FromSeconds(5);
        _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
        _dataReceivedByServer = new List<byte>();

        _server = new AsyncSocketListener(_serverEndPoint);
        _server.Start();
        _server.BytesReceived += (bytes, socket) =>
            {
                _dataReceivedByServer.AddRange(bytes);
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
            _protocolVersionExchange.Start(_clientVersion, _client, _timeout);
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
        Assert.NotNull(_actualException);
        Assert.Null(_actualException.InnerException);
        Assert.Equal(string.Format("The server response does not contain an SSH identification string.{0}" +
                                      "The connection to the remote server was closed before any data was received.{0}{0}" +
                                      "More information on the Protocol Version Exchange is available here:{0}" +
                                      "https://tools.ietf.org/html/rfc4253#section-4.2",
                                      Environment.NewLine),
                        _actualException.Message);
    }

    [Fact]
    public void ClientIdentificationWasSentToServer()
    {
        Assert.Equal(5, _dataReceivedByServer.Count);

        Assert.Equal(0xed, _dataReceivedByServer[0]);
        Assert.Equal(0x95, _dataReceivedByServer[1]);
        Assert.Equal(0x9c, _dataReceivedByServer[2]);
        Assert.Equal(0x0d, _dataReceivedByServer[3]);
        Assert.Equal(0x0a, _dataReceivedByServer[4]);
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
