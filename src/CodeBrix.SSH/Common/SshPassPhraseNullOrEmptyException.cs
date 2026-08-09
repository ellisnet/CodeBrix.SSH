using System;

namespace CodeBrix.SSH.Common; //was previously: Renci.SshNet.Common;

/// <summary>
/// The exception that is thrown when pass phrase for key file is empty or <see langword="null"/>.
/// </summary>
public class SshPassPhraseNullOrEmptyException : SshException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SshPassPhraseNullOrEmptyException"/> class.
    /// </summary>
    public SshPassPhraseNullOrEmptyException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SshPassPhraseNullOrEmptyException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public SshPassPhraseNullOrEmptyException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SshPassPhraseNullOrEmptyException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public SshPassPhraseNullOrEmptyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

}
