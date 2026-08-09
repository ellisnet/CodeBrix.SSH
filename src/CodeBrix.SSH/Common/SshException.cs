using System;

namespace CodeBrix.SSH.Common; //was previously: Renci.SshNet.Common;

/// <summary>
/// The exception that is thrown when an SSH exception occurs.
/// </summary>
public class SshException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SshException"/> class.
    /// </summary>
    public SshException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SshException"/> class.
    /// </summary>
    /// <inheritdoc cref="Exception(string, Exception)" path="/param"/>
    public SshException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SshException"/> class.
    /// </summary>
    /// <inheritdoc cref="Exception(string, Exception)" path="/param"/>
    public SshException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

}
