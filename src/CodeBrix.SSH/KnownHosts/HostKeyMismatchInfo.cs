namespace CodeBrix.SSH.KnownHosts;

/// <summary>
/// Describes a host key that failed verification against a <see cref="KnownHostsStore"/>,
/// passed to the mismatch callback of
/// <see cref="KnownHostsClientExtensions.UseTrustOnFirstUse(BaseClient, KnownHostsStore, System.Func{HostKeyMismatchInfo, bool})"/>.
/// </summary>
public sealed class HostKeyMismatchInfo
{
    /// <summary>
    /// Gets the host name or address the connection was made to.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Gets the port the connection was made to.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// Gets the host key algorithm name, e.g. <c>ssh-ed25519</c>.
    /// </summary>
    public string KeyAlgorithm { get; }

    /// <summary>
    /// Gets the host key blob presented by the server.
    /// </summary>
    public byte[] HostKey { get; }

    /// <summary>
    /// Gets the SHA256 fingerprint of the presented host key, in the same format as the
    /// <c>ssh</c> command (non-padded base64, without the <c>SHA256:</c> prefix).
    /// </summary>
    public string FingerPrintSHA256 { get; }

    /// <summary>
    /// Gets the verification result: <see cref="HostKeyVerificationResult.Mismatch"/> or
    /// <see cref="HostKeyVerificationResult.Revoked"/>.
    /// </summary>
    public HostKeyVerificationResult Result { get; }

    internal HostKeyMismatchInfo(string host, int port, string keyAlgorithm, byte[] hostKey, string fingerPrintSHA256, HostKeyVerificationResult result)
    {
        Host = host;
        Port = port;
        KeyAlgorithm = keyAlgorithm;
        HostKey = hostKey;
        FingerPrintSHA256 = fingerPrintSHA256;
        Result = result;
    }
}
