using System;
using CodeBrix.SSH.Connection;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Connection; //was previously: Renci.SshNet.Tests.Classes.Connection;

public class SshIdentificationTest
{
    [Fact]
    public void Ctor_ProtocolVersionAndSoftwareVersion()
    {
        const string protocolVersion = "1.5";
        const string softwareVersion = "SSH.NET_2020.0.0";

        var sshIdentification = new SshIdentification(protocolVersion, softwareVersion);
        Assert.Same(protocolVersion, sshIdentification.ProtocolVersion);
        Assert.Same(softwareVersion, sshIdentification.SoftwareVersion);
        Assert.Null(sshIdentification.Comments);
    }

    [Fact]
    public void Ctor_ProtocolVersionAndSoftwareVersion_ProtocolVersionIsNull()
    {
        const string protocolVersion = null;
        const string softwareVersion = "SSH.NET_2020.0.0";

        try
        {
            _ = new SshIdentification(protocolVersion, softwareVersion);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("protocolVersion", ex.ParamName);
        }
    }

    [Fact]
    public void Ctor_ProtocolVersionAndSoftwareVersion_SoftwareVersionIsNull()
    {
        const string protocolVersion = "2.0";
        const string softwareVersion = null;

        try
        {
            _ = new SshIdentification(protocolVersion, softwareVersion);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("softwareVersion", ex.ParamName);
        }
    }

    [Fact]
    public void Ctor_ProtocolVersionAndSoftwareVersionAndComments()
    {
        const string protocolVersion = "1.5";
        const string softwareVersion = "SSH.NET_2020.0.0";
        const string comments = "Beware, dangerous!";

        var sshIdentification = new SshIdentification(protocolVersion, softwareVersion, comments);
        Assert.Equal(protocolVersion, sshIdentification.ProtocolVersion);
        Assert.Equal(softwareVersion, sshIdentification.SoftwareVersion);
        Assert.Equal(comments, sshIdentification.Comments);
    }

    [Fact]
    public void Ctor_ProtocolVersionAndSoftwareVersionAndComments_CommentsIsNull()
    {
        const string protocolVersion = "1.5";
        const string softwareVersion = "SSH.NET_2020.0.0";
        const string comments = null;

        var sshIdentification = new SshIdentification(protocolVersion, softwareVersion, comments);
        Assert.Equal(protocolVersion, sshIdentification.ProtocolVersion);
        Assert.Equal(softwareVersion, sshIdentification.SoftwareVersion);
        Assert.Null(sshIdentification.Comments);
    }

    [Fact]
    public void Ctor_ProtocolVersionAndSoftwareVersionAndComments_ProtocolVersionIsNull()
    {
        const string protocolVersion = null;
        const string softwareVersion = "SSH.NET_2020.0.0";
        const string comments = "Beware!";

        try
        {
            _ = new SshIdentification(protocolVersion, softwareVersion, comments);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("protocolVersion", ex.ParamName);
        }
    }

    [Fact]
    public void Ctor_ProtocolVersionAndSoftwareVersionAndComments_SoftwareVersionIsNull()
    {
        const string protocolVersion = "2.0";
        const string softwareVersion = null;
        const string comments = "Beware!";

        try
        {
            _ = new SshIdentification(protocolVersion, softwareVersion, comments);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("softwareVersion", ex.ParamName);
        }
    }

    [Fact]
    public void ToString_Comments()
    {
        var sshIdentification = new SshIdentification("2.0", "CodeBrix.SSH", "Beware, dangerous");
        Assert.Equal("SSH-2.0-CodeBrix.SSH Beware, dangerous", sshIdentification.ToString());
    }

    [Fact]
    public void ToString_CommentsIsNull()
    {
        var sshIdentification = new SshIdentification("2.0", "SSH.NET_2020.0.0");
        Assert.Equal("SSH-2.0-SSH.NET_2020.0.0", sshIdentification.ToString());

        sshIdentification = new SshIdentification("2.0", "SSH.NET_2020.0.0", null);
        Assert.Equal("SSH-2.0-SSH.NET_2020.0.0", sshIdentification.ToString());
    }
}
