using System;
using System.Globalization;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Messages.Authentication;
using CodeBrix.SSH.Messages.Transport;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes; //was previously: Renci.SshNet.Tests.Classes;

public class SshMessageFactoryTest
{
    private SshMessageFactory _sshMessageFactory;

    public SshMessageFactoryTest()
    {
        SetUp();
    }

    private void SetUp()
    {
        _sshMessageFactory = new SshMessageFactory();
    }

    [Fact]
    public void CreateShouldThrowSshExceptionWhenMessageIsNotEnabled()
    {
        const byte messageNumber = 60;

        try
        {
            _sshMessageFactory.Create(messageNumber);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
        }
    }

    [Fact]
    public void CreateShouldThrowSshExceptionWhenMessageDoesNotExist_OutsideOfMessageNumberRange()
    {
        const byte messageNumber = 255;

        try
        {
            _sshMessageFactory.Create(messageNumber);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not supported.", messageNumber), ex.Message);
        }
    }

    [Fact]
    public void CreateShouldThrowSshExceptionWhenMessageDoesNotExist_WithinMessageNumberRange()
    {
        const byte messageNumber = 5;

        try
        {
            _sshMessageFactory.Create(messageNumber);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not supported.", messageNumber), ex.Message);
        }
    }

    [Fact]
    public void CreateShouldThrowSshExceptionWhenMessageIsNotActivated()
    {
        const byte messageNumber = 60;
        const string messageName = "SSH_MSG_USERAUTH_PASSWD_CHANGEREQ";

        _sshMessageFactory.EnableAndActivateMessage(messageName);
        _sshMessageFactory.DisableAndDeactivateMessage(messageName);

        try
        {
            _sshMessageFactory.Create(messageNumber);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
        }
    }

    [Fact]
    public void CreateShouldReturnMessageInstanceCorrespondingToMessageNumberWhenMessageIsEnabledAndActivated()
    {
        const byte messageNumber = 60;
        const string messageName = "SSH_MSG_USERAUTH_PASSWD_CHANGEREQ";

        _sshMessageFactory.EnableAndActivateMessage(messageName);

        var actual = _sshMessageFactory.Create(messageNumber);

        Assert.NotNull(actual);
        Assert.Equal(typeof(PasswordChangeRequiredMessage), actual.GetType());

        _sshMessageFactory.DisableAndDeactivateMessage(messageName);
        _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_INFO_REQUEST");

        actual = _sshMessageFactory.Create(messageNumber);

        Assert.NotNull(actual);
        Assert.Equal(typeof(InformationRequestMessage), actual.GetType());
    }

    [Fact]
    public void DisableAndDeactivateMessageShouldThrowSshExceptionWhenAnotherMessageWithSameMessageNumberIsEnabled()
    {
        _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");

        try
        {
            _sshMessageFactory.DisableAndDeactivateMessage("SSH_MSG_USERAUTH_INFO_REQUEST");
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("Cannot enable message 'SSH_MSG_USERAUTH_INFO_REQUEST'. Message type 60 is already enabled for 'SSH_MSG_USERAUTH_PASSWD_CHANGEREQ'.", ex.Message);
        }

        // verify that the original message remains enabled
        var actual = _sshMessageFactory.Create(60);
        Assert.NotNull(actual);
        Assert.Equal(typeof(PasswordChangeRequiredMessage), actual.GetType());
    }

    [Fact]
    public void DisableAndDeactivateMessageShouldNotThrowExceptionWhenMessageIsAlreadyDisabled()
    {
        const byte messageNumber = 60;

        _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
        _sshMessageFactory.DisableAndDeactivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
        _sshMessageFactory.DisableAndDeactivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");

        // verify that message remains disabled
        try
        {
            _sshMessageFactory.Create(messageNumber);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
        }
    }

    [Fact]
    public void DisableAndDeactivateMessageShouldNotThrowExceptionWhenMessageWasNeverEnabled()
    {
        const byte messageNumber = 60;

        _sshMessageFactory.DisableAndDeactivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");

        // verify that message is disabled
        try
        {
            _sshMessageFactory.Create(messageNumber);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
        }
    }

    [Fact]
    public void DisableAndDeactivateMessageShouldThrowSshExceptionWhenMessageIsNotSupported()
    {
        const string messageName = "WHATEVER";

        try
        {
            _sshMessageFactory.DisableAndDeactivateMessage("WHATEVER");
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal(string.Format("Message '{0}' is not supported.", messageName), ex.Message);
        }
    }

    [Fact]
    public void DisableAndDeactivateMessageShouldThrowArgumentNullExceptionWhenMessageNameIsNull()
    {
        const string messageName = null;

        try
        {
            _sshMessageFactory.DisableAndDeactivateMessage(messageName);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("messageName", ex.ParamName);
        }
    }

    [Fact]
    public void EnableAndActivateMessageShouldThrowSshExceptionWhenAnotherMessageWithSameMessageNumberIsAlreadyEnabled()
    {
        _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");

        try
        {
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_INFO_REQUEST");
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("Cannot enable message 'SSH_MSG_USERAUTH_INFO_REQUEST'. Message type 60 is already enabled for 'SSH_MSG_USERAUTH_PASSWD_CHANGEREQ'.", ex.Message);
        }

        // verify that the original message remains enabled
        var actual = _sshMessageFactory.Create(60);
        Assert.NotNull(actual);
        Assert.Equal(typeof(PasswordChangeRequiredMessage), actual.GetType());
    }

    [Fact]
    public void EnableAndActivateMessageShouldNotThrowExceptionWhenMessageIsAlreadyEnabled()
    {
        _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
        _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");

        var actual = _sshMessageFactory.Create(60);
        Assert.NotNull(actual);
        Assert.Equal(typeof(PasswordChangeRequiredMessage), actual.GetType());
    }

    [Fact]
    public void EnableAndActivateMessageShouldThrowSshExceptionWhenMessageIsNotSupported()
    {
        const string messageName = "WHATEVER";

        try
        {
            _sshMessageFactory.EnableAndActivateMessage("WHATEVER");
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal(string.Format("Message '{0}' is not supported.", messageName), ex.Message);
        }
    }

    [Fact]
    public void EnableAndActivateMessageShouldThrowArgumentNullExceptionWhenMessageNameIsNull()
    {
        const string messageName = null;

        try
        {
            _sshMessageFactory.EnableAndActivateMessage(messageName);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("messageName", ex.ParamName);
        }
    }

    [Fact]
    public void DisableNonKeyExchangeMessagesShouldDisableNonKeyExchangeMessages()
    {
        const byte messageNumber = 60;

        _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
        _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);

        // verify that message is disabled
        try
        {
            _sshMessageFactory.Create(messageNumber);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
        }
    }

    [Fact]
    public void DisableNonKeyExchangeMessagesShouldNotDisableKeyExchangeMessages()
    {
        const byte messageNumber = 21;

        _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_NEWKEYS");
        _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);

        // verify that message remains enabled
        var actual = _sshMessageFactory.Create(messageNumber);
        Assert.NotNull(actual);
        Assert.Equal(typeof(NewKeysMessage), actual.GetType());
    }

    [Fact]
    public void EnableActivatedMessagesShouldEnableMessagesThatWereEnabledPriorToInvokingDisableNonKeyExchangeMessages()
    {
        const byte messageNumber = 60;

        _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
        _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);
        _sshMessageFactory.EnableActivatedMessages();

        var actual = _sshMessageFactory.Create(messageNumber);
        Assert.NotNull(actual);
        Assert.Equal(typeof(PasswordChangeRequiredMessage), actual.GetType());
    }

    [Fact]
    public void EnableActivatedMessagesShouldNotEnableMessagesThatWereDisabledPriorToInvokingDisableNonKeyExchangeMessages()
    {
        const byte messageNumber = 60;

        _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);
        _sshMessageFactory.EnableActivatedMessages();

        try
        {
            _sshMessageFactory.Create(messageNumber);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
        }
    }

    [Fact]
    public void EnableActivatedMessagesShouldNotEnableMessagesThatWereDisabledAfterInvokingDisableNonKeyExchangeMessages()
    {
        const byte messageNumber = 60;

        _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
        _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);
        _sshMessageFactory.DisableAndDeactivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
        _sshMessageFactory.EnableActivatedMessages();

        try
        {
            _sshMessageFactory.Create(messageNumber);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
        }
    }

    [Fact]
    public void EnableActivatedMessagesShouldThrowSshExceptionWhenAnothersMessageWithSameMessageNumberWasEnabledAfterInvokingDisableNonKeyExchangeMessages()
    {
        _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
        _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);
        _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_INFO_REQUEST");

        try
        {
            _sshMessageFactory.EnableActivatedMessages();
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (SshException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("Cannot enable message 'SSH_MSG_USERAUTH_PASSWD_CHANGEREQ'. Message type 60 is already enabled for 'SSH_MSG_USERAUTH_INFO_REQUEST'.", ex.Message);
        }
    }

    [Fact]
    public void EnableActivatedMessagesShouldLeaveMessagesEnabledThatWereEnabledAfterInvokingDisableNonKeyExchangeMessages()
    {
        _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);
        _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
        _sshMessageFactory.EnableActivatedMessages();

        Assert.IsAssignableFrom<PasswordChangeRequiredMessage>(_sshMessageFactory.Create(60));
    }
}
