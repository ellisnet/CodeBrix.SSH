using System;
using Xunit;

namespace CodeBrix.SSH.Tests.Common; //was previously: Renci.SshNet.Tests.Common;

public static class ArgumentExceptionAssert
{
    public static void MessageEquals(string expected, ArgumentException exception)
    {
        var newMessage = new ArgumentException(expected, exception.ParamName);

        Assert.Equal(newMessage.Message, exception.Message);
    }
}
