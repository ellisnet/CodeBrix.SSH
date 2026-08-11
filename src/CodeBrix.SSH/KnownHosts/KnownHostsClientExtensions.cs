using System;
using CodeBrix.SSH.Common;

namespace CodeBrix.SSH.KnownHosts;

/// <summary>
/// Extension methods that wire a <see cref="KnownHostsStore"/> to a client's
/// <c>HostKeyReceived</c> event, providing the two common host key verification policies
/// without each application having to reimplement them.
/// </summary>
/// <remarks>
/// <para>
/// Call one of these methods once, after constructing the client and before calling
/// <c>Connect</c>. Each call adds an event handler, so calling more than once on the same
/// client stacks handlers (and the most restrictive outcome wins, since any handler can
/// veto trust).
/// </para>
/// <para>
/// These helpers verify plain host keys only. When the server presents a host
/// certificate, the certificate blob is treated like an ordinary key of the certificate
/// algorithm; <c>@cert-authority</c> entries in the store are not evaluated.
/// </para>
/// </remarks>
public static class KnownHostsClientExtensions
{
    /// <summary>
    /// Trusts only host keys already present in <paramref name="store"/>: unknown,
    /// changed and revoked keys are all rejected. The store is never modified.
    /// </summary>
    /// <param name="client">The client to protect.</param>
    /// <param name="store">The known-hosts store to verify against.</param>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> or <paramref name="store"/> is <see langword="null"/>.</exception>
    public static void UseStrictHostKeyVerification(this BaseClient client, KnownHostsStore store)
    {
        WireVerification(client, store, trustOnFirstUse: false, mismatchCallback: null);
    }

    /// <summary>
    /// Trust-on-first-use: a known key is trusted, an unknown key is trusted and appended
    /// to <paramref name="store"/> (which is saved immediately), and a changed or revoked
    /// key is rejected.
    /// </summary>
    /// <param name="client">The client to protect.</param>
    /// <param name="store">The known-hosts store to verify against and record first-use keys in.</param>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> or <paramref name="store"/> is <see langword="null"/>.</exception>
    public static void UseTrustOnFirstUse(this BaseClient client, KnownHostsStore store)
    {
        WireVerification(client, store, trustOnFirstUse: true, mismatchCallback: null);
    }

    /// <summary>
    /// Trust-on-first-use with a mismatch callback: a known key is trusted, an unknown key
    /// is trusted and appended to <paramref name="store"/> (which is saved immediately),
    /// and a changed or revoked key is passed to <paramref name="mismatchCallback"/> —
    /// typically to warn the user — whose return value decides whether the connection may
    /// proceed. Accepting a mismatch never modifies the store.
    /// </summary>
    /// <param name="client">The client to protect.</param>
    /// <param name="store">The known-hosts store to verify against and record first-use keys in.</param>
    /// <param name="mismatchCallback">Receives details of the failed verification; return <see langword="true"/> to trust the key for this session anyway, or <see langword="false"/> to reject it.</param>
    /// <exception cref="ArgumentNullException"><paramref name="client"/>, <paramref name="store"/> or <paramref name="mismatchCallback"/> is <see langword="null"/>.</exception>
    public static void UseTrustOnFirstUse(this BaseClient client, KnownHostsStore store, Func<HostKeyMismatchInfo, bool> mismatchCallback)
    {
        ThrowHelper.ThrowIfNull(mismatchCallback);

        WireVerification(client, store, trustOnFirstUse: true, mismatchCallback);
    }

    private static void WireVerification(BaseClient client, KnownHostsStore store, bool trustOnFirstUse, Func<HostKeyMismatchInfo, bool> mismatchCallback)
    {
        ThrowHelper.ThrowIfNull(client);
        ThrowHelper.ThrowIfNull(store);

        client.HostKeyReceived += (sender, e) =>
        {
            e.CanTrust = DecideTrust(store, client.ConnectionInfo.Host, client.ConnectionInfo.Port, e.HostKeyName, e.HostKey, e.FingerPrintSHA256, trustOnFirstUse, mismatchCallback);
        };
    }

    internal static bool DecideTrust(KnownHostsStore store, string host, int port, string keyAlgorithm, byte[] hostKey, string fingerPrintSHA256, bool trustOnFirstUse, Func<HostKeyMismatchInfo, bool> mismatchCallback)
    {
        var result = store.Verify(host, port, keyAlgorithm, hostKey);

        switch (result)
        {
            case HostKeyVerificationResult.Known:
                return true;

            case HostKeyVerificationResult.Unknown:
                if (trustOnFirstUse)
                {
                    store.Add(host, port, keyAlgorithm, hostKey);
                    store.Save();

                    return true;
                }

                return false;

            case HostKeyVerificationResult.Mismatch:
            case HostKeyVerificationResult.Revoked:
            default:
                return mismatchCallback != null
                    && mismatchCallback(new HostKeyMismatchInfo(host, port, keyAlgorithm, hostKey, fingerPrintSHA256, result));
        }
    }
}
