namespace CodeBrix.SSH.KnownHosts;

/// <summary>
/// The result of verifying a host key against a <see cref="KnownHostsStore"/>.
/// </summary>
public enum HostKeyVerificationResult
{
    /// <summary>
    /// The store contains no entry for this host and key algorithm.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The store contains an entry for this host and key algorithm with exactly this key.
    /// </summary>
    Known = 1,

    /// <summary>
    /// The store contains an entry for this host and key algorithm, but with a different key.
    /// This is the classic man-in-the-middle warning signal and should be treated as a
    /// failure unless the key is known to have been changed deliberately.
    /// </summary>
    Mismatch = 2,

    /// <summary>
    /// The store contains an <c>@revoked</c> entry for exactly this key. The key must not
    /// be accepted.
    /// </summary>
    Revoked = 3,
}
