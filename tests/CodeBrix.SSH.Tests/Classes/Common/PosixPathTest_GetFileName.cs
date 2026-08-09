using System;
using CodeBrix.SSH.Common;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Common; //was previously: Renci.SshNet.Tests.Classes.Common;

public class PosixPathTest_GetFileName
{
    [Fact]
    public void Path_Null()
    {
        const string path = null;

        try
        {
            _ = PosixPath.GetFileName(path);
            Assert.Fail("Test failed: reached code that should not have been reached.");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Null(ex.InnerException);
            Assert.Equal("path", ex.ParamName);
        }
    }

    [Fact]
    public void Path_Empty()
    {
        var path = string.Empty;

        var actual = PosixPath.GetFileName(path);

        Assert.NotNull(actual);
        Assert.Same(path, actual);
    }

    [Fact]
    public void Path_TrailingForwardSlash()
    {
        var path = "/abc/";

        var actual = PosixPath.GetFileName(path);

        Assert.NotNull(actual);
        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void Path_FileWithoutNoDirectory()
    {
        var path = "abc.log";

        var actual = PosixPath.GetFileName(path);

        Assert.NotNull(actual);
        Assert.Same(path, actual);
    }

    [Fact]
    public void Path_FileInRootDirectory()
    {
        var path = "/abc.log";

        var actual = PosixPath.GetFileName(path);

        Assert.NotNull(actual);
        Assert.Equal("abc.log", actual);
    }

    [Fact]
    public void Path_RootDirectoryOnly()
    {
        var path = "/";

        var actual = PosixPath.GetFileName(path);

        Assert.NotNull(actual);
        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void Path_FileInNonRootDirectory()
    {
        var path = "/home/sshnet/xyz";

        var actual = PosixPath.GetFileName(path);

        Assert.NotNull(actual);
        Assert.Equal("xyz", actual);
    }

    [Fact]
    public void Path_BackslashIsNotConsideredDirectorySeparator()
    {
        var path = "/home\\abc.log";

        var actual = PosixPath.GetFileName(path);

        Assert.NotNull(actual);
        Assert.Equal("home\\abc.log", actual);
    }

    [Fact]
    public void Path_ColonIsNotConsideredPathSeparator()
    {
        var path = "/home:abc.log";

        var actual = PosixPath.GetFileName(path);

        Assert.NotNull(actual);
        Assert.Equal("home:abc.log", actual);
    }

    [Fact]
    public void Path_LeadingWhitespace()
    {
        var path = "  / \tabc";

        var actual = PosixPath.GetFileName(path);

        Assert.NotNull(actual);
        Assert.Equal(" \tabc", actual);
    }

    [Fact]
    public void Path_TrailingWhitespace()
    {
        var path = "/abc \t ";

        var actual = PosixPath.GetFileName(path);

        Assert.NotNull(actual);
        Assert.Equal("abc \t ", actual);
    }

    [Fact]
    public void Path_OnlyWhitespace()
    {
        var path = " ";

        var actual = PosixPath.GetFileName(path);

        Assert.NotNull(actual);
        Assert.Equal(" ", actual);
    }

    [Fact]
    public void Path_FileNameOnlyWhitespace()
    {
        var path = "/home/\t ";

        var actual = PosixPath.GetFileName(path);

        Assert.NotNull(actual);
        Assert.Equal("\t ", actual);
    }
}
