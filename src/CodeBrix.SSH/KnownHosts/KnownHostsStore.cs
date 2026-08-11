using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CodeBrix.SSH.Common;

namespace CodeBrix.SSH.KnownHosts;

/// <summary>
/// A host key store backed by a file in the OpenSSH <c>known_hosts</c> format, so it can
/// read (and append to) the user's existing <c>~/.ssh/known_hosts</c> file or an
/// application-specific file of the same format.
/// </summary>
/// <remarks>
/// <para>
/// Supported when reading: plain host names, <c>[host]:port</c> entries, comma-separated
/// host lists, <c>*</c> and <c>?</c> wildcards, <c>!</c> negation, hashed host entries
/// (<c>|1|salt|hash</c>), comments, and the <c>@revoked</c> marker. Lines carrying the
/// <c>@cert-authority</c> marker are ignored (host certificates are outside the scope of
/// this store). Unparseable lines are ignored when verifying but preserved when saving.
/// </para>
/// <para>
/// Entries added with <see cref="Add"/> are written as plain (unhashed) host names.
/// Both LF and CRLF line endings are accepted when reading; LF is used when writing.
/// </para>
/// <para>
/// All members are thread-safe.
/// </para>
/// </remarks>
public sealed class KnownHostsStore
{
    private const int DefaultSshPort = 22;

    private readonly object _sync = new object();
    private readonly string _filePath;
    private readonly List<string> _lines = new List<string>();

    /// <summary>
    /// Gets the default path of the current user's OpenSSH <c>known_hosts</c> file:
    /// <c>~/.ssh/known_hosts</c> on Linux and macOS, and
    /// <c>%USERPROFILE%\.ssh\known_hosts</c> on Windows.
    /// </summary>
    public static string DefaultFilePath
    {
        get
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "known_hosts");
        }
    }

    /// <summary>
    /// Gets the path of the file this store reads from and saves to.
    /// </summary>
    public string FilePath
    {
        get { return _filePath; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KnownHostsStore"/> class, loading the
    /// specified file if it exists. A missing file is not an error; it is treated as an
    /// empty store and created on the first <see cref="Save"/>.
    /// </summary>
    /// <param name="filePath">The path of the <c>known_hosts</c>-format file to load and save.</param>
    /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is empty or white-space.</exception>
    public KnownHostsStore(string filePath)
    {
        ThrowHelper.ThrowIfNullOrWhiteSpace(filePath);

        _filePath = filePath;

        if (File.Exists(filePath))
        {
            _lines.AddRange(File.ReadAllLines(filePath));
        }
    }

    /// <summary>
    /// Verifies a host key against the store.
    /// </summary>
    /// <param name="host">The host name or address the connection was made to.</param>
    /// <param name="port">The port the connection was made to.</param>
    /// <param name="keyAlgorithm">The host key algorithm name, e.g. <c>ssh-ed25519</c>. This is the <c>HostKeyName</c> of the <c>HostKeyReceived</c> event.</param>
    /// <param name="key">The host key blob. This is the <c>HostKey</c> of the <c>HostKeyReceived</c> event.</param>
    /// <returns>
    /// <see cref="HostKeyVerificationResult.Known"/> if an entry matches the key exactly;
    /// <see cref="HostKeyVerificationResult.Revoked"/> if an <c>@revoked</c> entry matches the key exactly;
    /// <see cref="HostKeyVerificationResult.Mismatch"/> if an entry exists for the host and
    /// algorithm but with a different key; otherwise <see cref="HostKeyVerificationResult.Unknown"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="host"/>, <paramref name="keyAlgorithm"/> or <paramref name="key"/> is <see langword="null"/>.</exception>
    public HostKeyVerificationResult Verify(string host, int port, string keyAlgorithm, byte[] key)
    {
        ThrowHelper.ThrowIfNull(host);
        ThrowHelper.ThrowIfNull(keyAlgorithm);
        ThrowHelper.ThrowIfNull(key);

        var candidateNames = GetCandidateNames(host, port);

        var known = false;
        var mismatch = false;

        lock (_sync)
        {
            foreach (var line in _lines)
            {
                if (!TryParseLine(line, out var marker, out var hostPatterns, out var lineKeyType, out var lineKey))
                {
                    continue;
                }

                if (string.Equals(marker, "@cert-authority", StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.Equals(lineKeyType, keyAlgorithm, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!MatchesHost(hostPatterns, candidateNames))
                {
                    continue;
                }

                var revoked = string.Equals(marker, "@revoked", StringComparison.Ordinal);

                if (KeysEqual(lineKey, key))
                {
                    if (revoked)
                    {
                        return HostKeyVerificationResult.Revoked;
                    }

                    known = true;
                }
                else if (!revoked)
                {
                    mismatch = true;
                }
            }
        }

        if (known)
        {
            return HostKeyVerificationResult.Known;
        }

        return mismatch ? HostKeyVerificationResult.Mismatch : HostKeyVerificationResult.Unknown;
    }

    /// <summary>
    /// Adds a host key entry to the in-memory store. Call <see cref="Save"/> to persist it.
    /// </summary>
    /// <param name="host">The host name or address.</param>
    /// <param name="port">The port. Port 22 is written as a plain host name; any other port is written in the <c>[host]:port</c> form.</param>
    /// <param name="keyAlgorithm">The host key algorithm name, e.g. <c>ssh-ed25519</c>.</param>
    /// <param name="key">The host key blob.</param>
    /// <exception cref="ArgumentNullException"><paramref name="host"/>, <paramref name="keyAlgorithm"/> or <paramref name="key"/> is <see langword="null"/>.</exception>
    public void Add(string host, int port, string keyAlgorithm, byte[] key)
    {
        ThrowHelper.ThrowIfNull(host);
        ThrowHelper.ThrowIfNull(keyAlgorithm);
        ThrowHelper.ThrowIfNull(key);

        var name = FormatName(host, port);

        lock (_sync)
        {
            _lines.Add(name + " " + keyAlgorithm + " " + Convert.ToBase64String(key));
        }
    }

    /// <summary>
    /// Saves the store to <see cref="FilePath"/>, creating the directory and file if
    /// needed. On Linux and macOS a newly created directory is given owner-only (700)
    /// permissions and a newly created file owner-only (600) permissions, matching
    /// OpenSSH conventions; on Windows no explicit permissions are set.
    /// </summary>
    public void Save()
    {
        lock (_sync)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(_filePath));

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                _ = Directory.CreateDirectory(directory);

                if (!OperatingSystem.IsWindows())
                {
                    File.SetUnixFileMode(directory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
                }
            }

            var fileExisted = File.Exists(_filePath);

            var contents = new StringBuilder();

            foreach (var line in _lines)
            {
                _ = contents.Append(line).Append('\n');
            }

            File.WriteAllText(_filePath, contents.ToString());

            if (!fileExisted && !OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(_filePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }
    }

    private static string[] GetCandidateNames(string host, int port)
    {
        var lowerHost = host.Trim().ToLowerInvariant();

        if (port == DefaultSshPort)
        {
            // OpenSSH writes port-22 entries as a plain host name, but also accepts
            // the explicit "[host]:22" form; match both.
            return [lowerHost, "[" + lowerHost + "]:" + DefaultSshPort];
        }

        return ["[" + lowerHost + "]:" + port];
    }

    private static string FormatName(string host, int port)
    {
        var lowerHost = host.Trim().ToLowerInvariant();

        return port == DefaultSshPort ? lowerHost : "[" + lowerHost + "]:" + port;
    }

    private static bool TryParseLine(string line, out string marker, out string[] hostPatterns, out string keyType, out byte[] key)
    {
        marker = null;
        hostPatterns = null;
        keyType = null;
        key = null;

        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var trimmed = line.Trim();

        if (trimmed.StartsWith('#'))
        {
            return false;
        }

        var tokens = trimmed.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        var index = 0;

        if (tokens[0].StartsWith('@'))
        {
            marker = tokens[0];
            index = 1;
        }

        if (tokens.Length - index < 3)
        {
            return false;
        }

        hostPatterns = tokens[index].Split(',', StringSplitOptions.RemoveEmptyEntries);
        keyType = tokens[index + 1];

        var keyBuffer = new byte[((tokens[index + 2].Length * 3) / 4) + 2];

        if (!Convert.TryFromBase64String(tokens[index + 2], keyBuffer, out var bytesWritten))
        {
            return false;
        }

        key = keyBuffer.AsSpan(0, bytesWritten).ToArray();

        return true;
    }

    private static bool MatchesHost(string[] hostPatterns, string[] candidateNames)
    {
        var positiveMatch = false;

        foreach (var pattern in hostPatterns)
        {
            var negated = pattern.StartsWith('!');
            var effectivePattern = negated ? pattern.Substring(1) : pattern;

            var matches = false;

            foreach (var name in candidateNames)
            {
                if (MatchesPattern(effectivePattern, name))
                {
                    matches = true;
                    break;
                }
            }

            if (!matches)
            {
                continue;
            }

            if (negated)
            {
                // A matching negated pattern excludes the whole line for this host.
                return false;
            }

            positiveMatch = true;
        }

        return positiveMatch;
    }

    private static bool MatchesPattern(string pattern, string name)
    {
        if (pattern.StartsWith("|1|", StringComparison.Ordinal))
        {
            return MatchesHashedPattern(pattern, name);
        }

        if (pattern.Contains('*') || pattern.Contains('?'))
        {
            var regexPattern = "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

            return Regex.IsMatch(name, regexPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));
        }

        return string.Equals(pattern, name, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesHashedPattern(string pattern, string name)
    {
        var parts = pattern.Split('|');

        if (parts.Length != 4)
        {
            return false;
        }

        byte[] salt;
        byte[] hash;

        try
        {
            salt = Convert.FromBase64String(parts[2]);
            hash = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var computed = HMACSHA1.HashData(salt, Encoding.UTF8.GetBytes(name));

        return CryptographicOperations.FixedTimeEquals(computed, hash);
    }

    private static bool KeysEqual(byte[] left, byte[] right)
    {
        return left.AsSpan().SequenceEqual(right);
    }
}
