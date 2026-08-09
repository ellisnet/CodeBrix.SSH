using System;
using CodeBrix.SSH.Common;
using CodeBrix.SSH.Sftp;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Common; //was previously: Renci.SshNet.Tests.Classes.Common;

public class SftpExceptionTest
{
    [Fact]
    public void StatusCodes()
    {
        Assert.Equal(StatusCode.BadMessage, new SftpException(StatusCode.BadMessage).StatusCode);
        Assert.Equal(StatusCode.OperationUnsupported, new SftpException(StatusCode.OperationUnsupported, null).StatusCode);
        Assert.Equal(StatusCode.Failure, new SftpException(StatusCode.Failure, null, null).StatusCode);

        Assert.Equal(StatusCode.PermissionDenied, new SftpPermissionDeniedException().StatusCode);
        Assert.Equal(StatusCode.PermissionDenied, new SftpPermissionDeniedException(null).StatusCode);
        Assert.Equal(StatusCode.PermissionDenied, new SftpPermissionDeniedException(null, null).StatusCode);

        Assert.Equal(StatusCode.NoSuchFile, new SftpPathNotFoundException().StatusCode);
        Assert.Equal(StatusCode.NoSuchFile, new SftpPathNotFoundException(null).StatusCode);
        Assert.Equal(StatusCode.NoSuchFile, new SftpPathNotFoundException(null, path: null).StatusCode);
        Assert.Equal(StatusCode.NoSuchFile, new SftpPathNotFoundException(null, innerException: null).StatusCode);
        Assert.Equal(StatusCode.NoSuchFile, new SftpPathNotFoundException(null, null, null).StatusCode);
    }

    [Fact]
    public void Message()
    {
        Assert.False(string.IsNullOrWhiteSpace(new SftpException(StatusCode.Failure).Message));
        Assert.False(string.IsNullOrWhiteSpace(new SftpException(StatusCode.Failure, "").Message));
        Assert.Equal("Custom message", new SftpException(StatusCode.Failure, "Custom message").Message);

        Assert.False(string.IsNullOrWhiteSpace(new SftpPermissionDeniedException().Message));
        Assert.False(string.IsNullOrWhiteSpace(new SftpPermissionDeniedException("").Message));
        Assert.False(string.IsNullOrWhiteSpace(new SftpPermissionDeniedException("", null).Message));
        Assert.Equal("Custom message1", new SftpPermissionDeniedException("Custom message1").Message);
        Assert.Equal("Custom message2", new SftpPermissionDeniedException("Custom message2", null).Message);

        Assert.False(string.IsNullOrWhiteSpace(new SftpPathNotFoundException().Message));
        Assert.False(string.IsNullOrWhiteSpace(new SftpPathNotFoundException("").Message));
        Assert.False(string.IsNullOrWhiteSpace(new SftpPathNotFoundException("", path: null).Message));
        Assert.Equal("Custom message1", new SftpPathNotFoundException("Custom message1").Message);
        Assert.Equal("Custom message2", new SftpPathNotFoundException("Custom message2", path: null).Message);
        Assert.Equal("Custom message2", new SftpPathNotFoundException("Custom message2", "path1").Message);
        Assert.Equal("Custom message3", new SftpPathNotFoundException("Custom message3", innerException: null).Message);
        Assert.Equal("Custom message4", new SftpPathNotFoundException("Custom message4", null, null).Message);
    }

    [Fact]
    public void PathNotFoundException_Path()
    {
        Assert.Null(new SftpPathNotFoundException().Path);
        Assert.Null(new SftpPathNotFoundException("message").Path);
        Assert.Equal("path1", new SftpPathNotFoundException("message", "path1").Path);
        Assert.Equal("path2", new SftpPathNotFoundException(null, "path2", null).Path);

        Assert.Contains("Path: 'path3'.", new SftpPathNotFoundException(message: null, path: "path3").Message, StringComparison.Ordinal);
        Assert.Contains("Path: 'path4'.", new SftpPathNotFoundException(message: "", path: "path4").Message, StringComparison.Ordinal);
    }
}
