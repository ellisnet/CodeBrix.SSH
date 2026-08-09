using System;
using CodeBrix.SSH.Sftp;

namespace CodeBrix.SSH.Common; //was previously: Renci.SshNet.Common;

/// <summary>
/// The exception that is thrown when operation permission is denied.
/// </summary>
public class SftpPermissionDeniedException : SftpException
{
    private const StatusCode Code = StatusCode.PermissionDenied;

    /// <summary>
    /// Initializes a new instance of the <see cref="SftpPermissionDeniedException"/> class.
    /// </summary>
    public SftpPermissionDeniedException()
        : base(Code)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SftpPermissionDeniedException"/> class.
    /// </summary>
    /// <inheritdoc cref="Exception(string)" path="/param"/>
    public SftpPermissionDeniedException(string message)
        : base(Code, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SftpPermissionDeniedException"/> class.
    /// </summary>
    /// <inheritdoc cref="Exception(string, Exception)" path="/param"/>
    public SftpPermissionDeniedException(string message, Exception innerException)
        : base(Code, message, innerException)
    {
    }

}
