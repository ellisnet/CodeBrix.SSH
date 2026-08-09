using System;
using System.Text;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using CodeBrix.SSH.Sftp.Responses;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Sftp; //was previously: Renci.SshNet.Tests.Classes.Sftp;

public class SftpSessionTest_Connected_RequestStatVfs
{
    #region SftpSession.Connect()

    private Mock<ISession> _sessionMock;
    private Mock<IChannelSession> _channelSessionMock;
    private ISftpResponseFactory _sftpResponseFactory;
    private SftpSession _sftpSession;
    private int _operationTimeout;
    private Encoding _encoding;
    private uint _protocolVersion;
    private SftpVersionResponse _sftpVersionResponse;
    private SftpNameResponse _sftpNameResponse;
    private byte[] _sftpInitRequestBytes;
    private byte[] _sftpRealPathRequestBytes;

    #endregion SftpSession.Connect()

    private byte[] _sftpStatVfsRequestBytes;
    private StatVfsResponse _sftpStatVfsResponse;
    private ulong _bAvail;
    private string _path;
    private SftpFileSystemInformation _actual;

    public SftpSessionTest_Connected_RequestStatVfs()
    {
        Setup();
    }

    private void Setup()
    {
        Arrange();
        Act();
    }

    private void SetupData()
    {
        var random = new Random();

        #region SftpSession.Connect()

        _operationTimeout = random.Next(100, 500);
        _encoding = Encoding.UTF8;
        _protocolVersion = 3;
        _sftpResponseFactory = new SftpResponseFactory();
        _sftpInitRequestBytes = new SftpInitRequestBuilder().WithVersion(SftpSession.MaximumSupportedVersion)
                                                            .Build()
                                                            .GetBytes();
        _sftpVersionResponse = new SftpVersionResponseBuilder().WithVersion(_protocolVersion)
                                                               .WithExtension("statvfs@openssh.com", "")
                                                               .Build();
        _sftpRealPathRequestBytes = new SftpRealPathRequestBuilder().WithProtocolVersion(_protocolVersion)
                                                                    .WithRequestId(1)
                                                                    .WithPath(".")
                                                                    .WithEncoding(_encoding)
                                                                    .Build()
                                                                    .GetBytes();
        _sftpNameResponse = new SftpNameResponseBuilder().WithProtocolVersion(_protocolVersion)
                                                         .WithResponseId(1U)
                                                         .WithEncoding(_encoding)
                                                         .WithFile("ABC", SftpFileAttributesBuilder.Empty)
                                                         .Build();

        #endregion SftpSession.Connect()

        _path = random.Next().ToString();
        _bAvail = (ulong)random.Next(0, int.MaxValue);
        _sftpStatVfsRequestBytes = new SftpStatVfsRequestBuilder().WithProtocolVersion(_protocolVersion)
                                                                  .WithRequestId(2)
                                                                  .WithPath(_path)
                                                                  .WithEncoding(_encoding)
                                                                  .Build()
                                                                  .GetBytes();
        _sftpStatVfsResponse = new SftpStatVfsResponseBuilder().WithProtocolVersion(_protocolVersion)
                                                               .WithResponseId(2U)
                                                               .WithBAvail(_bAvail)
                                                               .Build();
    }

    private void CreateMocks()
    {
        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
    }

    private void SetupMocks()
    {
        var sequence = new MockSequence();

        #region SftpSession.Connect()

        _ = _sessionMock.InSequence(sequence)
                        .Setup(p => p.CreateChannelSession())
                        .Returns(_channelSessionMock.Object);
        _ = _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.Open());
        _ = _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.SendSubsystemRequest("sftp"))
                               .Returns(true);
        _ = _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.IsOpen)
                               .Returns(true);
        _ = _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.SendData(_sftpInitRequestBytes))
                               .Callback(() =>
                                    {
                                        _channelSessionMock.Raise(c => c.DataReceived += null,
                                                                  new ChannelDataEventArgs(0, _sftpVersionResponse.GetBytes()));
                                    });
        _ = _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.IsOpen)
                               .Returns(true);
        _ = _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.SendData(_sftpRealPathRequestBytes))
                               .Callback(() =>
                                   {
                                       _channelSessionMock.Raise(c => c.DataReceived += null,
                                                                 new ChannelDataEventArgs(0, _sftpNameResponse.GetBytes()));
                                   });

        #endregion SftpSession.Connect()

        _ = _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.IsOpen)
                               .Returns(true);
        _ = _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.SendData(_sftpStatVfsRequestBytes))
                               .Callback(() =>
                                   {
                                       _channelSessionMock.Raise(c => c.DataReceived += null,
                                                                 new ChannelDataEventArgs(0, _sftpStatVfsResponse.GetBytes()));
                                   });
    }

    protected void Arrange()
    {
        SetupData();
        CreateMocks();
        SetupMocks();

        _sftpSession = new SftpSession(_sessionMock.Object, _operationTimeout, _encoding, _sftpResponseFactory);
        _sftpSession.Connect();
    }

    protected void Act()
    {
        _actual = _sftpSession.RequestStatVfs(_path);
    }

    [Fact]
    public void ReturnedValueShouldNotBeNull()
    {
        Assert.NotNull(_actual);
    }

    [Fact]
    public void AvailableBlocksInReturnedValueShouldMatchValueInSftpResponse()
    {
        Assert.Equal(_bAvail, _actual.AvailableBlocks);
    }
}
