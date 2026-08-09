using System;
using System.Globalization;
using System.Text;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Connection;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SshCommandTest_EndExecute_ChannelOpen : TestBase
{
    private Mock<ISession> _sessionMock;
    private Mock<IChannelSession> _channelSessionMock;
    private string _commandText;
    private Encoding _encoding;
    private SshCommand _sshCommand;
    private IAsyncResult _asyncResult;
    private string _actual;
    private string _dataA;
    private string _dataB;
    private string _extendedDataA;
    private string _extendedDataB;
    private int _expectedExitStatus;

    protected override void OnInit()
    {
        base.OnInit();

        Arrange();
        Act();
    }

    private void Arrange()
    {
        var random = new Random();

        _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
        _commandText = random.Next().ToString(CultureInfo.InvariantCulture);
        _encoding = Encoding.UTF8;
        _expectedExitStatus = random.Next();
        _dataA = random.Next().ToString(CultureInfo.InvariantCulture);
        _dataB = random.Next().ToString(CultureInfo.InvariantCulture);
        _extendedDataA = random.Next().ToString(CultureInfo.InvariantCulture);
        _extendedDataB = random.Next().ToString(CultureInfo.InvariantCulture);
        _asyncResult = null;

        var seq = new MockSequence();
        _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
        _channelSessionMock.InSequence(seq).Setup(p => p.Open());
        _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText))
            .Returns(true);
        _channelSessionMock.InSequence(seq).Setup(p => p.Dispose());

        _sshCommand = new SshCommand(_sessionMock.Object, _commandText, _encoding);
        _asyncResult = _sshCommand.BeginExecute();

        _channelSessionMock.Raise(c => c.DataReceived += null,
            new ChannelDataEventArgs(0, _encoding.GetBytes(_dataA)));
        _channelSessionMock.Raise(c => c.ExtendedDataReceived += null,
            new ChannelExtendedDataEventArgs(0, _encoding.GetBytes(_extendedDataA), 0));
        _channelSessionMock.Raise(c => c.DataReceived += null,
            new ChannelDataEventArgs(0, _encoding.GetBytes(_dataB)));
        _channelSessionMock.Raise(c => c.ExtendedDataReceived += null,
            new ChannelExtendedDataEventArgs(0, _encoding.GetBytes(_extendedDataB), 0));
        _channelSessionMock.Raise(c => c.RequestReceived += null,
            new ChannelRequestEventArgs(new ExitStatusRequestInfo((uint)_expectedExitStatus)));
        _channelSessionMock.Raise(c => c.Closed += null,
            new ChannelEventArgs(5));
    }

    private void Act()
    {
        _actual = _sshCommand.EndExecute(_asyncResult);
    }

    [Fact]
    public void EndExecuteShouldReturnAllDataReceivedInSpecifiedEncoding()
    {
        Assert.Equal(string.Concat(_dataA, _dataB), _actual);
    }

    [Fact]
    public void EndExecuteShouldNotThrowWhenInvokedAgainWithSameAsyncResult()
    {
        Assert.Equal(_sshCommand.Result, _sshCommand.EndExecute(_asyncResult));
    }

    [Fact]
    public void ErrorShouldReturnZeroLengthString()
    {
        Assert.Equal(string.Empty, _sshCommand.Error);
    }

    [Fact]
    public void ExitStatusShouldReturnExitStatusFromExitStatusRequestInfo()
    {
        Assert.Equal(_expectedExitStatus, _sshCommand.ExitStatus);
    }

    [Fact]
    public void ExtendedOutputStreamShouldContainAllExtendedDataReceived()
    {
        var extendedDataABytes = _encoding.GetBytes(_extendedDataA);
        var extendedDataBBytes = _encoding.GetBytes(_extendedDataB);

        var extendedOutputStream = _sshCommand.ExtendedOutputStream;
        Assert.Equal(extendedDataABytes.Length + extendedDataBBytes.Length, extendedOutputStream.Length);

        var buffer = new byte[extendedOutputStream.Length];
        var bytesRead = extendedOutputStream.Read(buffer, 0, buffer.Length);

        Assert.Equal(buffer.Length, bytesRead);
        Assert.Equal(string.Concat(_extendedDataA, _extendedDataB), _encoding.GetString(buffer));
        Assert.Equal(0, extendedOutputStream.Length);
    }

    [Fact]
    public void OutputStreamShouldBeEmpty()
    {
        Assert.Equal(0, _sshCommand.OutputStream.Length);
    }

    [Fact]
    public void ResultShouldReturnAllDataReceived()
    {
        Assert.Equal(string.Concat(_dataA, _dataB), _sshCommand.Result);
    }
}
