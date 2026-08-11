================================================================================
AGENT-README: CodeBrix.SSH
A Comprehensive Guide for AI Coding Agents
================================================================================


OVERVIEW
--------------------------------------------------------------------------------

CodeBrix.SSH is a fully managed, cross-platform Secure Shell (SSH-2) library
for .NET 10. It provides SSH command execution, an interactive shell, SFTP,
SCP, and local / remote / dynamic (SOCKS) port forwarding, along with a broad
set of key exchange, cipher, MAC and host key algorithms and support for
OpenSSH, PKCS#1, PKCS#8, PuTTY and ssh.com private key formats.

CodeBrix.SSH is a fork of SSH.NET 2025.1.0 (https://github.com/sshnet/SSH.NET),
narrowed to net10.0 and rehomed under the CodeBrix.SSH namespace. Every public
type keeps its SSH.NET name, so migrating an existing SSH.NET consumer is
normally a package swap plus a namespace find-and-replace.

Six upstream fixes made after the 2025.1.0 release are also applied, so the
public surface is a small SUPERSET of SSH.NET 2025.1.0 (see THIRD-PARTY-NOTICES
for the commit list):

  * ScpClient.Download now truncates the local file, rather than leaving stale
    trailing bytes when overwriting a larger existing file.
  * ScpClient.UseDirectoryFlag opts out of the undocumented "-d" scp flag, for
    servers (such as Cisco) that reject it. Defaults to true.
  * SftpClient.UploadFileAsync has a canOverride overload.
  * SftpClient.DownloadFileAsync and UploadFileAsync accept an
    IProgress&lt;DownloadFileProgressReport&gt; / IProgress&lt;UploadFileProgressReport&gt;.

Beyond those upstream fixes, the fork carries CodeBrix-original additions with
no SSH.NET counterpart (marked "not in SSH.NET" where documented below):

  * ShellStream conveniences for interactive terminal hosts: AutoFlush,
    WriteAndFlush, and the opt-in DisableReadBuffering. See the ShellStream
    section.
  * The CodeBrix.SSH.KnownHosts namespace: an OpenSSH known_hosts file store
    plus ready-made strict and trust-on-first-use host key verification
    policies. See "Host key verification" in the CORE API REFERENCE.


INSTALLATION
--------------------------------------------------------------------------------

NuGet package:  CodeBrix.SSH.MitLicenseForever

    dotnet add package CodeBrix.SSH.MitLicenseForever

Note that the ".MitLicenseForever" suffix is part of the PACKAGE ID only. The
assembly, the root namespace and every type live under "CodeBrix.SSH" without
the suffix.

Target framework: .NET 10.0 or higher. There is no multi-targeting; net10.0 is
the only supported target and netstandard / .NET Framework are not supported.

NuGet dependencies (2):

  CodeBrix.Cryptography.MitLicenseForever   Used for Diffie-Hellman key
                                            agreement, Ed25519 and ML-KEM /
                                            sntrup761 key exchange, ChaCha20 /
                                            Poly1305, Argon2 and PKCS#8 private
                                            key decryption.

                                            This is the CodeBrix fork of
                                            BouncyCastle.NET. Upstream SSH.NET
                                            depends on BouncyCastle.Cryptography;
                                            this library does NOT. The API is
                                            the same, but every namespace is
                                            rooted at CodeBrix.Cryptography
                                            rather than Org.BouncyCastle -- so
                                            write

                                              using CodeBrix.Cryptography.Crypto.Parameters;

                                            and never `using Org.BouncyCastle...`.
                                            Do not add a BouncyCastle package
                                            reference back.

  Microsoft.Extensions.Logging.Abstractions Used for the library's internal
                                            diagnostic logging. Logging is a
                                            no-op unless a logger factory is
                                            supplied.


KEY NAMESPACE
--------------------------------------------------------------------------------

    using CodeBrix.SSH;

Sub-namespaces, which map one-to-one onto the SSH.NET originals except for
CodeBrix.SSH.KnownHosts (new in this fork):

    CodeBrix.SSH                    Clients, connection info, authentication
                                    methods, forwarded ports, shell types.
    CodeBrix.SSH.Common             Exceptions, event args, extension methods,
                                    the Ssh* helper types.
    CodeBrix.SSH.Sftp               SFTP file, attribute and stream types.
    CodeBrix.SSH.Security           Key exchange, host key algorithms, keys.
    CodeBrix.SSH.Security.Cryptography
                                    Digital signatures, key implementations.
    CodeBrix.SSH.Security.Cryptography.Ciphers
                                    Cipher implementations and modes.
    CodeBrix.SSH.Messages           SSH protocol message types.
    CodeBrix.SSH.Channels           SSH channel implementations.
    CodeBrix.SSH.Connection         Direct and proxied connectors.
    CodeBrix.SSH.Compression        Compression algorithms.
    CodeBrix.SSH.KnownHosts         known_hosts host key verification (new in
                                    this fork; no SSH.NET counterpart).
    CodeBrix.SSH.Abstractions       Internal platform abstractions.
    CodeBrix.SSH.NetConf            NETCONF over SSH.

MIGRATING FROM SSH.NET: replace "Renci.SshNet" with "CodeBrix.SSH" in every
using directive and fully-qualified name. Nothing else should need to change.


CORE API REFERENCE
--------------------------------------------------------------------------------

SshClient
    Connects to an SSH server and runs commands or opens shells.

        using var client = new SshClient("host", "user", "password");
        client.Connect();

        using SshCommand cmd = client.RunCommand("uname -a");
        Console.WriteLine(cmd.Result);
        Console.WriteLine(cmd.ExitStatus);

    Key members: Connect / ConnectAsync, Disconnect, IsConnected, RunCommand,
    CreateCommand, CreateShell, CreateShellStream, AddForwardedPort,
    RemoveForwardedPort, ForwardedPorts, ConnectionInfo, KeepAliveInterval.

    KeepAliveInterval enables periodic keep-alive messages on an otherwise
    idle connection. Set it BEFORE calling Connect(); 30 seconds is a
    reasonable value for a long-lived interactive session.

    RunCommand opens a fresh exec channel per call on the already-
    authenticated connection, so it is safe to call while an interactive
    ShellStream is live on the same client: probing commands and an open
    terminal multiplex on separate channels without interfering with each
    other in either direction.

SftpClient
    SFTP file transfer and remote file system operations. Synchronous and
    asynchronous variants exist for essentially every operation.

        using var client = new SftpClient("host", "user", "password");
        client.Connect();

        await client.UploadFileAsync(stream, "/remote/path.txt", cancellationToken);

        await foreach (ISftpFile file in client.ListDirectoryAsync("/remote"))
        {
            Console.WriteLine(file.FullName);
        }

    Key members: UploadFile, DownloadFile, ListDirectory, CreateDirectory,
    DeleteFile, DeleteDirectory, RenameFile, Exists, Get, GetAttributes,
    SetAttributes, Open / Create / AppendText and the *Async counterparts,
    WorkingDirectory, BufferSize, OperationTimeout.

    Progress reporting on the async transfer methods (not present in SSH.NET
    2025.1.0):

        var progress = new Progress&lt;DownloadFileProgressReport&gt;(
            r =&gt; Console.WriteLine(r.TotalBytesDownloaded));

        await client.DownloadFileAsync("/remote/file", stream, progress, cancellationToken);
        await client.UploadFileAsync(stream, "/remote/file", canOverride: false,
                                     uploadProgress: null, cancellationToken);

    Note that the canOverride overload takes uploadProgress before the
    cancellation token, so pass the token by name when omitting the progress.

ScpClient
    SCP upload and download of files and directories.

    Key members: Upload, Download, Uploading, Downloading events,
    RemotePathTransformation, UseDirectoryFlag, BufferSize.

PrivateKeyFile
    Loads a private key from a stream, file path or PEM/OpenSSH text, with an
    optional passphrase. Supports RSA, ECDSA (256/384/521) and ED25519 across
    the OpenSSL traditional PEM, PKCS#8, ssh.com, OpenSSH and PuTTY formats,
    plus OpenSSH certificates.

        var key = new PrivateKeyFile("/path/to/id_ed25519", "passphrase");
        using var client = new SshClient("host", "user", key);

ConnectionInfo and its subclasses
    PasswordConnectionInfo, PrivateKeyConnectionInfo,
    KeyboardInteractiveConnectionInfo, and the AuthenticationMethod hierarchy,
    give full control over multi-factor authentication, proxies (SOCKS4,
    SOCKS5, HTTP), timeouts, and the negotiated algorithm sets
    (Encryptions, HmacAlgorithms, KeyExchangeAlgorithms, HostKeyAlgorithms,
    CompressionAlgorithms).

ForwardedPortLocal / ForwardedPortRemote / ForwardedPortDynamic
    Local, remote and dynamic (SOCKS) port forwarding. Added to a client with
    AddForwardedPort and started with Start.

ShellStream
    A Stream over an interactive shell session, obtained from
    SshClient.CreateShellStream. It serves two distinct consumption styles:
    scripted automation (Expect / ExpectAsync, ReadLine, WriteLine) and
    interactive terminal hosting (Read / Write plus the DataReceived event).

        using ShellStream shell = client.CreateShellStream(
            "xterm-256color", columns: 120, rows: 30, width: 0, height: 0,
            bufferSize: 4096);

    The terminalName argument is passed through verbatim and becomes TERM on
    the remote side.

    INTERACTIVE CONSUMPTION PATTERN (terminal hosts). Use a dedicated reader
    thread in a blocking Read() loop. Read() blocks until output arrives and
    returns 0 when the channel closes, which doubles as the disconnect
    signal:

        var buffer = new byte[4096];
        int n;
        while ((n = shell.Read(buffer, 0, buffer.Length)) > 0)
        {
            terminal.Feed(buffer, 0, n);    // render the chunk
        }
        // n == 0: channel closed -- tear down the session UI here.

    The ErrorOccurred event supplies a human-readable reason to show when the
    read loop ends abnormally, and Closed fires when the channel closes.

    PITFALL -- DataReceived does not replace Read(). Every incoming chunk is
    committed to the internal read buffer AND raised via DataReceived; the
    two are not alternatives. A consumer that subscribes to DataReceived and
    never calls Read() leaves every byte of session output accumulating in
    the internal read buffer for the life of the session, growing it without
    bound. Event-style consumers must opt in to DisableReadBuffering (below),
    or simply use the Read() loop above.

    DisableReadBuffering (not in SSH.NET; opt-in, default false). When set to
    true, incoming data is delivered solely via DataReceived and is never
    committed to the internal read buffer, eliminating the unbounded growth
    described above. While it is set, the buffer-reading members (Read,
    ReadLine, Expect, BeginExpect and friends) throw
    InvalidOperationException rather than block on a buffer that will never
    fill -- it is strictly for event-style consumers. Set it once,
    immediately after CreateShellStream. Setting it to true discards
    anything already buffered; setting it back to false resumes buffering
    from that point.

    WRITES ARE BUFFERED -- call Flush to send. The byte-oriented writes
    (Write(byte[], int, int), Write(ReadOnlySpan<byte>), WriteByte,
    WriteAsync) accumulate in a write buffer and reach the wire only when
    Flush() is called or the buffer fills. Nothing fails when Flush is
    forgotten; the session just sits silent. The string-oriented
    Write(string) and WriteLine(string) always flush automatically. Two
    conveniences (not in SSH.NET) remove the boilerplate for terminal hosts,
    where every keystroke batch must be flushed:

        shell.AutoFlush = true;           // byte-oriented writes now flush
        shell.WriteAndFlush(bytes, 0, n); // explicit write-plus-flush
        shell.WriteAndFlush(text);        //   (byte[] and string overloads)

    ChangeWindowSize(columns, rows, width, height) sends the PTY
    window-change request for live terminal resizing. It is easy to assume
    missing (SSH.NET lore includes long-open "no public resize"
    discussions), but it is present and works; call it whenever the hosting
    control is resized. After Read/Write it is the most important
    ShellStream member for interactive use.

Host key verification (CodeBrix.SSH.KnownHosts -- new in this fork)
    By default nothing verifies the server's host key; the client's
    HostKeyReceived event decides. Accept-all is one line and acceptable for
    a dev tool:

        client.HostKeyReceived += (_, e) => e.CanTrust = true;

    For the ground between "verify nothing" and hand-written known-hosts
    logic, the CodeBrix.SSH.KnownHosts namespace provides a store and two
    ready-made policies, as extension methods on any client (SSH, SFTP,
    SCP):

        using CodeBrix.SSH.KnownHosts;

        var store = new KnownHostsStore(KnownHostsStore.DefaultFilePath);

        client.UseTrustOnFirstUse(store);              // TOFU
        client.UseStrictHostKeyVerification(store);    // known keys only

        client.UseTrustOnFirstUse(store, mismatch =>
        {
            // Changed or revoked key -- warn, return true to proceed anyway.
            ShowHostKeyWarning(mismatch.Host, mismatch.Port,
                               mismatch.FingerPrintSHA256, mismatch.Result);
            return false;
        });

    Call one policy method per client, after construction and before
    Connect(). Strict trusts only keys already in the store and never writes
    to it. Trust-on-first-use trusts known keys, trusts and records unknown
    keys (saving the store immediately), and rejects changed (Mismatch) or
    revoked (Revoked) keys -- or defers those to the callback overload,
    whose return value decides; accepting a mismatch never modifies the
    store.

    KnownHostsStore reads and writes the OpenSSH known_hosts format: plain
    and [host]:port entries, comma-separated host lists, * and ? wildcards,
    ! negation, hashed |1| host lines and the @revoked marker are honored
    when reading (@cert-authority lines are ignored -- host certificates are
    out of scope). Entries it appends are plain-hostname lines. Point it at
    the user's own file -- DefaultFilePath resolves ~/.ssh/known_hosts on
    Linux/macOS and %USERPROFILE%\.ssh\known_hosts on Windows (the OpenSSH
    client bundled with Windows uses that same location) -- or at an
    application-specific file, which Save() creates on demand, with
    owner-only permissions on Linux/macOS. Verify/Add/Save are thread-safe,
    and both LF and CRLF files are accepted.

SshNetLoggingConfiguration
    Static configuration for the library's internal logging.

        SshNetLoggingConfiguration.InitializeLogging(loggerFactory);

    Per-connection logging can be set instead via ConnectionInfo.LoggerFactory.

Error model
    All library exceptions derive from CodeBrix.SSH.Common.SshException:
    SshConnectionException, SshAuthenticationException, SshOperationTimeoutException,
    SshPassPhraseNullOrEmptyException, SftpPathNotFoundException,
    SftpPermissionDeniedException, ScpException, ProxyException,
    NetConfServerException.


CODING CONVENTIONS (CodeBrix family)
--------------------------------------------------------------------------------

These rules apply to every change made to this repository.

  * Target framework is net10.0 only. Do not add target frameworks and do not
    reintroduce framework-selection #if directives.

  * Nullable reference types are OFF. Never write `?` on a reference type
    (`string?`, `MyClass?`, `object?`) and never use the null-forgiveness `!`
    operator. Value-type nullables (`int?`, `bool?`, `DateTime?`) are fine.
    Do not add `#nullable` directives or a <Nullable> csproj property.

  * No implicit usings and no `global using`. Every file declares the usings
    it needs.

  * File-scoped namespaces only (`namespace X;`), never block-scoped.

  * Usings sit above the namespace in a single contiguous block with no blank
    lines inside it: System.* first, then everything else, alphabetical within
    each group, with aliases last.

  * Every file ported from SSH.NET carries a provenance comment on its
    namespace line:

        namespace CodeBrix.SSH.Sftp; //was previously: Renci.SshNet.Sftp;

    Keep it when editing a ported file. New-in-fork files do not get one.

  * <GenerateDocumentationFile> is true and warnings are never suppressed.
    Every public and protected member needs an XML doc comment; fix CS1591 by
    writing the documentation, never by adding <NoWarn>.

  * The build must be 0 warnings / 0 errors. Do not add <NoWarn>,
    <WarningLevel>, or #pragma warning disable to silence anything.

  * Tests use xUnit.v3 with CodeBrix.TestMocks for mocking and SilverAssertions
    where an assertion needs to carry a diagnostic message. Do NOT add Moq --
    CodeBrix.TestMocks is a Moq fork with the same API under the
    CodeBrix.TestMocks.Mocking namespace.

  * Any call inside a test that accepts a CancellationToken must be passed
    TestContext.Current.CancellationToken, or xUnit1051 fires.

  * Test parallelisation is disabled assembly-wide (see
    tests/CodeBrix.SSH.Tests/Properties/AssemblyInfo.cs). Many tests bind
    listeners to fixed local ports; running collections in parallel makes them
    fail with "Address already in use". Do not remove that attribute.

  * These tests are a port of the upstream SSH.NET suite, so they keep the
    upstream test class and method names rather than the CodeBrix
    <ClassUnderTest>Tests / snake_case convention. Follow the surrounding file
    when adding to an existing test class.


ARCHITECTURE
--------------------------------------------------------------------------------

    src/CodeBrix.SSH/
        *.cs                    Public entry points: SshClient, SftpClient,
                                ScpClient, PrivateKeyFile, ConnectionInfo,
                                authentication methods, forwarded ports,
                                Session, ShellStream, SshCommand.
        Abstractions/           Internal platform and crypto abstractions.
        Channels/               SSH channel implementations (session,
                                direct-tcpip, forwarded-tcpip, x11).
        Common/                 Exceptions, event args, extension methods,
                                buffered streams, the ASN.1/DER readers.
        Compression/            zlib and zlib@openssh.com.
        Connection/             Direct, HTTP-proxy and SOCKS connectors.
        Messages/               SSH wire protocol messages, grouped into
                                Authentication, Connection and Transport.
        NetConf/                NETCONF-over-SSH subsystem session.
        Security/               Key exchange algorithms, host keys, key
                                implementations, ciphers, MACs, and the
                                Cryptography sub-tree.
        Sftp/                   SFTP session, file, attribute and stream
                                types, with Requests/ and Responses/ holding
                                the SFTP wire protocol messages.
        Properties/             Assembly-level CLSCompliant declaration.
        InternalsVisibleTo.cs   Grants internal access to the test project and
                                to DynamicProxy (needed by CodeBrix.TestMocks).

    tests/CodeBrix.SSH.Tests/
        Mirrors the source layout, plus:
        Common/                 Shared test helpers and base classes.
        Data/                   Embedded key, certificate and sample data
                                fixtures used by the tests.

Version numbering is date-stamped by MSBuild at build time
(1.<years-since-2026>.<day-of-year>.<minute-of-day>), so every build produces a
new version. Do not add a literal <Version> to the csproj.


TESTING
--------------------------------------------------------------------------------

    dotnet test CodeBrix.SSH.slnx

The suite is a port of the SSH.NET unit tests to xUnit.v3, and is entirely
self-contained: it runs offline, needs no SSH server, and needs no Docker. The
upstream integration tests (which require Testcontainers and a live sshd) and
the benchmark projects are deliberately not part of this repository.

Mocking uses CodeBrix.TestMocks (namespace CodeBrix.TestMocks.Mocking), not Moq.
The API is the same -- Mock<T>, It, Times, MockBehavior -- so upstream SSH.NET
test code carries over with only the using directive changed.

Tests exercise internal types directly, which works because
InternalsVisibleTo.cs grants access to CodeBrix.SSH.Tests. CodeBrix.TestMocks
mocks internal interfaces such as ISession and ISftpSession, and its
DynamicProxy fork emits proxies into an assembly named DynamicProxyGenAssembly2,
which is why InternalsVisibleTo.cs also grants access to that name — do not
remove that line.

Test parallelisation is disabled for the whole assembly. Many of these tests
bind listeners to fixed local ports, so running collections concurrently makes
them fail with "Address already in use".

KNOWN FLAKE -- `dotnet test` occasionally exits non-zero while still reporting
every test as passing, with a line like:

    [xUnit.net] Catastrophic failure: System.Net.Sockets.SocketException : Broken pipe

The socket-based tests tear down real sockets on background threads, and a peer
can drop a connection at just the wrong moment. MSTest ignored exceptions raised
on non-test threads; xUnit installs a global unhandled-exception hook and treats
them as a fatal run error, even though no test failed. The exception varies
between runs (Broken pipe, ObjectDisposedException) and no stack trace reaches
the VSTest adapter.

It has only ever been observed through the VSTest bridge. Running the test
assembly directly with the native xUnit v3 runner has been consistently clean:

    tests/CodeBrix.SSH.Tests/bin/Debug/net10.0/CodeBrix.SSH.Tests

Prefer that command when a reliable exit code matters, such as in CI.

================================================================================
