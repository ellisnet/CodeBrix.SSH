using System;

namespace CodeBrix.SSH.Common; //was previously: Renci.SshNet.Common;

/// <summary>
/// The exception that is thrown when operation is timed out.
/// </summary>
public class SshOperationTimeoutException : SshException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SshOperationTimeoutException"/> class.
    /// </summary>
    public SshOperationTimeoutException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SshOperationTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public SshOperationTimeoutException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SshOperationTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public SshOperationTimeoutException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

}
