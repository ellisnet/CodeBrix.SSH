using System;

namespace CodeBrix.SSH.Common; //was previously: Renci.SshNet.Common;

/// <summary>
/// The exception that is thrown when there is something wrong with the server capabilities.
/// </summary>
public class NetConfServerException : SshException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NetConfServerException"/> class.
    /// </summary>
    public NetConfServerException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NetConfServerException"/> class.
    /// </summary>
    /// <inheritdoc cref="Exception(string)" path="/param"/>
    public NetConfServerException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NetConfServerException"/> class.
    /// </summary>
    /// <inheritdoc cref="Exception(string, Exception)" path="/param"/>
    public NetConfServerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

}
