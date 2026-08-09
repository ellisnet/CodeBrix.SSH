using System;

namespace CodeBrix.SSH; //was previously: Renci.SshNet;

/// <summary>
/// Supports port forwarding functionality.
/// </summary>
public interface IForwardedPort : IDisposable
{
    /// <summary>
    /// The <see cref="Closing"/> event occurs as the forwarded port is being stopped.
    /// </summary>
    event EventHandler Closing;
}
