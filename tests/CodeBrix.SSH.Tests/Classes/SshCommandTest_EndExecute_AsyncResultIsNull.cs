using System;
using System.Globalization;
using System.Text;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SshCommandTest_EndExecute_AsyncResultIsNull : TestBase
{
    private Mock<ISession> _sessionMock;
    private string _commandText;
    private Encoding _encoding;
    private SshCommand _sshCommand;
    private IAsyncResult _asyncResult;
    private ArgumentNullException _actualException;

    protected override void OnInit()
    {
        base.OnInit();

        Arrange();
        Act();
    }

    private void Arrange()
    {
        _sessionMock = new Mock<ISession>();
        _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
        _commandText = new Random().Next().ToString(CultureInfo.InvariantCulture);
        _encoding = Encoding.UTF8;
        _asyncResult = null;

        _sshCommand = new SshCommand(_sessionMock.Object, _commandText, _encoding);
    }

    private void Act()
    {
        try
        {
            _sshCommand.EndExecute(_asyncResult);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            _actualException = ex;
        }
    }

    [Fact]
    public void EndExecuteShouldHaveThrownArgumentNullException()
    {
        Assert.NotNull(_actualException);
        Assert.Null(_actualException.InnerException);
        Assert.Equal("asyncResult", _actualException.ParamName);
    }
}
