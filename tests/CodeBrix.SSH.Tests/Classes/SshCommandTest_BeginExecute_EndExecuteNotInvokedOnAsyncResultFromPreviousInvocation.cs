using System;
using System.Globalization;
using System.Text;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SshCommandTest_BeginExecute_EndExecuteNotInvokedOnAsyncResultFromPreviousInvocation : TestBase
{
    private Mock<ISession> _sessionMock;
    private Mock<IChannelSession> _channelSessionMock;
    private string _commandText;
    private Encoding _encoding;
    private SshCommand _sshCommand;
    private InvalidOperationException _actualException;

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

        var seq = new MockSequence();
        _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
        _channelSessionMock.InSequence(seq).Setup(p => p.Open());
        _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText)).Returns(true);

        _sshCommand = new SshCommand(_sessionMock.Object, _commandText, _encoding);
        _sshCommand.BeginExecute();
    }

    private void Act()
    {
        try
        {
            _sshCommand.BeginExecute();
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (InvalidOperationException ex)
        {
            _actualException = ex;
        }
    }

    [Fact]
    public void BeginExecuteShouldThrowInvalidOperationException()
    {
        Assert.NotNull(_actualException);
        Assert.Null(_actualException.InnerException);
        Assert.Equal("Asynchronous operation is already in progress.", _actualException.Message);
    }
}
