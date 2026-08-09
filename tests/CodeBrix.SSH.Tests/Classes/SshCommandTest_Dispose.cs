using System;
using System.Globalization;
using System.IO;
using System.Text;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SshCommandTest_Dispose : TestBase
{
    private Mock<ISession> _sessionMock;
    private Mock<IChannelSession> _channelSessionMock;
    private string _commandText;
    private Encoding _encoding;
    private SshCommand _sshCommand;
    private Stream _outputStream;
    private Stream _extendedOutputStream;

    protected override void OnInit()
    {
        base.OnInit();

        Arrange();
        Act();
    }

    private void Arrange()
    {
        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        _commandText = new Random().Next().ToString(CultureInfo.InvariantCulture);
        _encoding = Encoding.UTF8;
        _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);

        var seq = new MockSequence();

        _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
        _channelSessionMock.InSequence(seq).Setup(p => p.Open());
        _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText)).Returns(true);
        _channelSessionMock.InSequence(seq).Setup(p => p.Dispose());

        _sshCommand = new SshCommand(_sessionMock.Object, _commandText, _encoding);
        _sshCommand.BeginExecute();

        _outputStream = _sshCommand.OutputStream;
        _extendedOutputStream = _sshCommand.ExtendedOutputStream;
    }

    private void Act()
    {
        _sshCommand.Dispose();
    }

    [Fact]
    public void ChannelSessionShouldBeDisposedOnce()
    {
        _channelSessionMock.Verify(p => p.Dispose(), Times.Once);
    }

    [Fact]
    public void OutputStreamShouldHaveBeenDisposed()
    {
        Assert.Equal(-1, _outputStream.ReadByte());
    }

    [Fact]
    public void ExtendedOutputStreamShouldHaveBeenDisposed()
    {
        Assert.Equal(-1, _extendedOutputStream.ReadByte());
    }

    [Fact]
    public void RaisingDisconnectedOnSessionShouldDoNothing()
    {
        _sessionMock.Raise(s => s.Disconnected += null, new EventArgs());
    }

    [Fact]
    public void RaisingErrorOccurredOnSessionShouldDoNothing()
    {
        _sessionMock.Raise(s => s.ErrorOccured += null, new ExceptionEventArgs(new Exception()));
    }

    [Fact]
    public void InvokingDisposeAgainShouldNotRaiseAnException()
    {
        _sshCommand.Dispose();
    }
}
