using System;

namespace CodeBrix.SSH.Common; //was previously: Renci.SshNet.Common;

/// <summary>
/// The exception that is thrown when an SCP error occurs.
/// </summary>
public class ScpException : SshException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScpException"/> class.
    /// </summary>
    public ScpException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScpException"/> class.
    /// </summary>
    /// <inheritdoc cref="Exception(string)" path="/param"/>
    public ScpException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScpException"/> class.
    /// </summary>
    /// <inheritdoc cref="Exception(string, Exception)" path="/param"/>
    public ScpException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

}
