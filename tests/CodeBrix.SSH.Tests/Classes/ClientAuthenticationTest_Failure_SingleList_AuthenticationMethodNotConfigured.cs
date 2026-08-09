using System.Collections.Generic;
using CodeBrix.SSH.Common;
using CodeBrix.TestMocks.Mocking;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class ClientAuthenticationTest_Failure_SingleList_AuthenticationMethodNotSupported : ClientAuthenticationTestBase
{
    private int _partialSuccessLimit;
    private ClientAuthentication _clientAuthentication;
    private SshAuthenticationException _actualException;

    protected override void SetupData()
    {
        _partialSuccessLimit = 1;
    }

    protected override void SetupMocks()
    {
        var seq = new MockSequence();

        SessionMock.InSequence(seq).Setup(p => p.RegisterMessage("SSH_MSG_USERAUTH_FAILURE"));
        SessionMock.InSequence(seq).Setup(p => p.RegisterMessage("SSH_MSG_USERAUTH_SUCCESS"));
        SessionMock.InSequence(seq).Setup(p => p.RegisterMessage("SSH_MSG_USERAUTH_BANNER"));

        ConnectionInfoMock.InSequence(seq).Setup(p => p.CreateNoneAuthenticationMethod())
            .Returns(NoneAuthenticationMethodMock.Object);

        NoneAuthenticationMethodMock.InSequence(seq)
                                    .Setup(p => p.Authenticate(SessionMock.Object))
                                    .Returns(AuthenticationResult.Failure);
        ConnectionInfoMock.InSequence(seq).Setup(p => p.AuthenticationMethods)
                          .Returns(new List<IAuthenticationMethod>
                              {
                                  PublicKeyAuthenticationMethodMock.Object
                              });
        NoneAuthenticationMethodMock.InSequence(seq)
                                    .Setup(p => p.AllowedAuthentications)
                                    .Returns(new[] { "password" });

        PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");

        SessionMock.InSequence(seq).Setup(p => p.UnRegisterMessage("SSH_MSG_USERAUTH_FAILURE"));
        SessionMock.InSequence(seq).Setup(p => p.UnRegisterMessage("SSH_MSG_USERAUTH_SUCCESS"));
        SessionMock.InSequence(seq).Setup(p => p.UnRegisterMessage("SSH_MSG_USERAUTH_BANNER"));
    }

    protected override void Arrange()
    {
        base.Arrange();

        _clientAuthentication = new ClientAuthentication(_partialSuccessLimit);
    }

    protected override void Act()
    {
        try
        {
            _clientAuthentication.Authenticate(ConnectionInfoMock.Object, SessionMock.Object);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshAuthenticationException ex)
        {
            _actualException = ex;
        }
    }

    [Fact]
    public void AuthenticateShouldThrowSshAuthenticationException()
    {
        Assert.NotNull(_actualException);
        Assert.Null(_actualException.InnerException);
        Assert.Equal("No suitable authentication method found to complete authentication (password).", _actualException.Message);
    }
}
