using System;
using CodeBrix.SSH.Messages.Transport;

namespace CodeBrix.SSH.Common; //was previously: Renci.SshNet.Common;

/// <summary>
/// The exception that is thrown when connection was terminated.
/// </summary>
public class SshConnectionException : SshException
{
    /// <summary>
    /// Gets the disconnect reason if provided by the server or client. Otherwise None.
    /// </summary>
    public DisconnectReason DisconnectReason { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
    /// </summary>
    public SshConnectionException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public SshConnectionException(string message)
        : base(message)
    {
        DisconnectReason = DisconnectReason.None;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="disconnectReasonCode">The disconnect reason code.</param>
    public SshConnectionException(string message, DisconnectReason disconnectReasonCode)
        : base(message)
    {
        DisconnectReason = disconnectReasonCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="inner">The inner.</param>
    public SshConnectionException(string message, Exception inner)
        : base(message, inner)
    {
        DisconnectReason = DisconnectReason.None;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="disconnectReasonCode">The disconnect reason code.</param>
    /// <param name="inner">The inner.</param>
    public SshConnectionException(string message, DisconnectReason disconnectReasonCode, Exception inner)
        : base(message, inner)
    {
        DisconnectReason = disconnectReasonCode;
    }

}
