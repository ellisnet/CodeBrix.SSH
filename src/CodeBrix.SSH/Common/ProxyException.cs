using System;

namespace CodeBrix.SSH.Common; //was previously: Renci.SshNet.Common;

/// <summary>
/// The exception that is thrown when a proxy connection cannot be established.
/// </summary>
public class ProxyException : SshException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyException"/> class.
    /// </summary>
    public ProxyException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyException"/> class.
    /// </summary>
    /// <inheritdoc cref="Exception(string)" path="/param"/>
    public ProxyException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyException"/> class.
    /// </summary>
    /// <inheritdoc cref="Exception(string, Exception)" path="/param"/>
    public ProxyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

}
