using System;
using CodeBrix.SSH.Tests.Common;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ClientAuthenticationTest
{
    private ClientAuthentication _clientAuthentication;

    public ClientAuthenticationTest()
    {
        Init();
    }

    private void Init()
    {
        _clientAuthentication = new ClientAuthentication(1);
    }

    [Fact]
    public void Ctor_PartialSuccessLimit_Zero()
    {
        const int partialSuccessLimit = 0;

        try
        {
            new ClientAuthentication(partialSuccessLimit);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Assert.Null(ex.InnerException);
            ArgumentExceptionAssert.MessageEquals("Cannot be less than one.", ex);
            Assert.Equal("partialSuccessLimit", ex.ParamName);
        }
    }

    [Fact]
    public void Ctor_PartialSuccessLimit_Negative()
    {
        var partialSuccessLimit = new Random().Next(int.MinValue, -1);

        try
        {
            new ClientAuthentication(partialSuccessLimit);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Assert.Null(ex.InnerException);
            ArgumentExceptionAssert.MessageEquals("Cannot be less than one.", ex);
            Assert.Equal("partialSuccessLimit", ex.ParamName);
        }
    }

    [Fact]
    public void Ctor_PartialSuccessLimit_One()
    {
        const int partialSuccessLimit = 1;

        var clientAuthentication = new ClientAuthentication(partialSuccessLimit);
        Assert.Equal(partialSuccessLimit, clientAuthentication.PartialSuccessLimit);
    }

    [Fact]
    public void Ctor_PartialSuccessLimit_MaxValue()
    {
        const int partialSuccessLimit = int.MaxValue;

        var clientAuthentication = new ClientAuthentication(partialSuccessLimit);
        Assert.Equal(partialSuccessLimit, clientAuthentication.PartialSuccessLimit);
    }

    [Fact]
    public void AuthenticateShouldThrowArgumentNullExceptionWhenConnectionInfoIsNull()
    {
        const IConnectionInfoInternal connectionInfo = null;
        var session = new Mock<ISession>(MockBehavior.Strict).Object;

        try
        {
            _clientAuthentication.Authenticate(connectionInfo, session);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("connectionInfo", ex.ParamName);
        }
    }

    [Fact]
    public void AuthenticateShouldThrowArgumentNullExceptionWhenSessionIsNull()
    {
        var connectionInfo = new Mock<IConnectionInfoInternal>(MockBehavior.Strict).Object;
        const ISession session = null;

        try
        {
            _clientAuthentication.Authenticate(connectionInfo, session);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("session", ex.ParamName);
        }
    }
}
