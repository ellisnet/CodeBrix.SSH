using System;
using CodeBrix.SSH.Connection;

namespace CodeBrix.SSH.Common; //was previously: Renci.SshNet.Common;

/// <summary>
/// Provides data for the ServerIdentificationReceived events.
/// </summary>
public class SshIdentificationEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SshIdentificationEventArgs"/> class.
    /// </summary>
    /// <param name="sshIdentification">The SSH identification.</param>
    public SshIdentificationEventArgs(SshIdentification sshIdentification)
    {
        SshIdentification = sshIdentification;
    }

    /// <summary>
    /// Gets the SSH identification.
    /// </summary>
    public SshIdentification SshIdentification { get; private set; }
}
