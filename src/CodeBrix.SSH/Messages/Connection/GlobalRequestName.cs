namespace CodeBrix.SSH.Messages.Connection; //was previously: Renci.SshNet.Messages.Connection;

/// <summary>
/// Specifies supported request names.
/// </summary>
public enum GlobalRequestName
{
    /// <summary>
    /// tcpip-forward.
    /// </summary>
    TcpIpForward,

    /// <summary>
    /// cancel-tcpip-forward.
    /// </summary>
    CancelTcpIpForward,
}
