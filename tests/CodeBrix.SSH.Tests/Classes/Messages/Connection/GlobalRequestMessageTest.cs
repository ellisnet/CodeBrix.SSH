using System;
using System.Globalization;
using System.Text;
using CodeBrix.SSH.Messages.Connection;
using CodeBrix.SSH.Tests.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Messages.Connection; //was previously: Renci.SshNet.Tests.Classes.Messages.Connection;

/// <summary>
///This is a test class for GlobalRequestMessageTest and is intended
///to contain all GlobalRequestMessageTest Unit Tests
///</summary>
public class GlobalRequestMessageTest : TestBase
{
    [Fact]
    public void DefaultCtor()
    {
        new GlobalRequestMessage();
    }

    [Fact]
    public void Ctor_RequestNameAndWantReply()
    {
        var requestName = new Random().Next().ToString(CultureInfo.InvariantCulture);

        var target = new GlobalRequestMessage(Encoding.ASCII.GetBytes(requestName), true);
        Assert.Equal(requestName, target.RequestName);
        Assert.True(target.WantReply);

        target = new GlobalRequestMessage(Encoding.ASCII.GetBytes(requestName), false);
        Assert.Equal(requestName, target.RequestName);
        Assert.False(target.WantReply);
    }
}
