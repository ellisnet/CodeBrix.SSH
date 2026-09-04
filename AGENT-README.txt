================================================================================
AGENT-README: CodeBrix.SSH
A Guide for AI Coding Agents — CONSUMING the CodeBrix.SSH.MitLicenseForever
NuGet package
================================================================================


OVERVIEW
================================================================================

CodeBrix.SSH is a fully managed, cross-platform Secure Shell (SSH-2) client
library. It provides remote command execution, an interactive shell (with or
without a pseudo-terminal), SFTP file transfer and remote file system
operations, SCP upload and download, local / remote / dynamic (SOCKS) port
forwarding, and a NETCONF-over-SSH subsystem client. It ships a broad set of
key exchange, cipher, MAC and host key algorithms, and reads private keys in
the OpenSSH, PKCS#1 (OpenSSL traditional PEM), PKCS#8, PuTTY and ssh.com
formats, including OpenSSH certificates.

Target framework: .NET 10 or later. There is no multi-targeting; net10.0 is
the only supported target, and netstandard / .NET Framework are not supported.

Everything is managed code -- there are no native binaries, no P/Invoke into
an OpenSSH or libssh installation, and no OS-specific behaviour beyond the
default known_hosts path. The library runs anywhere .NET 10 runs.

PROVENANCE. CodeBrix.SSH is a fork of SSH.NET
(https://github.com/sshnet/SSH.NET), narrowed to net10.0 and rehomed under the
CodeBrix.SSH root namespace. The exact upstream release this fork is based on
is recorded in THIRD-PARTY-NOTICES.txt. Every public type keeps its upstream name, and
every sub-namespace maps one-to-one (Renci.SshNet.X becomes CodeBrix.SSH.X),
so migrating an existing SSH.NET consumer is normally a package swap plus a
find-and-replace of "Renci.SshNet" to "CodeBrix.SSH" in using directives and
fully-qualified names. Do NOT write upstream namespaces; they do not exist in
this package.

The public surface is a small SUPERSET of the upstream SSH.NET surface.
Additions a consumer can rely on:

  * ScpClient.Download truncates the local file rather than leaving stale
    trailing bytes when overwriting a larger existing file.
  * ScpClient.UseDirectoryFlag (default true) opts out of the undocumented
    "-d" scp flag, for servers (such as Cisco) that reject it.
  * SftpClient.UploadFileAsync has a canOverride overload.
  * SftpClient.DownloadFileAsync and UploadFileAsync accept an
    IProgress<DownloadFileProgressReport> / IProgress<UploadFileProgressReport>.
  * ShellStream conveniences for interactive terminal hosts: AutoFlush,
    WriteAndFlush, and the opt-in DisableReadBuffering.
  * The CodeBrix.SSH.KnownHosts namespace: an OpenSSH known_hosts file store
    plus ready-made strict and trust-on-first-use host key verification
    policies.

Two behavioural notes carried by the rename:

  * The SSH protocol version-exchange string this client sends is
    "SSH-2.0-CodeBrix.SSH.SshClient.<version>".
  * Logger categories are derived from type names, so they are rooted at
    "CodeBrix.SSH.*".


INSTALLATION
================================================================================

NuGet package id:  CodeBrix.SSH.MitLicenseForever

    dotnet add package CodeBrix.SSH.MitLicenseForever

The ".MitLicenseForever" suffix is part of the PACKAGE ID only. The assembly,
the root namespace and every type live under "CodeBrix.SSH" without the
suffix.

License: MIT.

NuGet dependencies (2):

  CodeBrix.Cryptography.MitLicenseForever
      Used for Diffie-Hellman key agreement, Ed25519, ML-KEM and sntrup761
      key exchange, ChaCha20 / Poly1305, Argon2 (PuTTY v3 keys) and PKCS#8
      private key decryption.

      WARNING -- NAMESPACES. This is the CodeBrix fork of BouncyCastle.NET.
      Upstream SSH.NET depends on BouncyCastle.Cryptography; this library does
      NOT, and no BouncyCastle package appears anywhere in the dependency
      graph. The API is the same, but every namespace is rooted at
      CodeBrix.Cryptography rather than Org.BouncyCastle -- so write

          using CodeBrix.Cryptography.Crypto.Parameters;

      and never `using Org.BouncyCastle...`. Do not add a BouncyCastle package
      reference back; doing so pulls in a second, incompatible copy of the
      same types.

  Microsoft.Extensions.Logging.Abstractions
      Used for the library's internal diagnostic logging. Logging is a no-op
      unless a logger factory is supplied.

No other requirements: no native libraries, no OS packages, no elevated
permissions.


KEY NAMESPACES / USINGS
================================================================================

    using CodeBrix.SSH;             // clients, connection info, auth methods,
                                    // forwarded ports, PrivateKeyFile,
                                    // SshCommand, Shell, ShellStream
    using CodeBrix.SSH.Common;      // exceptions, event args, TerminalModes
    using CodeBrix.SSH.Sftp;        // ISftpFile, SftpFile, SftpFileAttributes,
                                    // SftpFileStream, SftpFileSystemInformation
    using CodeBrix.SSH.KnownHosts;  // KnownHostsStore and the verification
                                    // policy extension methods

The complete namespace map, with the number of public types in each:

    CodeBrix.SSH                       Clients (SshClient, SftpClient,
                                       ScpClient, NetConfClient, BaseClient),
                                       ConnectionInfo and subclasses,
                                       authentication methods, forwarded
                                       ports, PrivateKeyFile, SshCommand,
                                       Shell, ShellStream, ExpectAction,
                                       ProxyTypes, RemotePathTransformation.
    CodeBrix.SSH.Common                Exception hierarchy, event args,
                                       TerminalModes, PipeStream, SshData /
                                       SshDataStream.
    CodeBrix.SSH.Sftp                  SFTP file, attribute, stream and
                                       file-system-information types, plus
                                       StatusCode.
    CodeBrix.SSH.KnownHosts            known_hosts store, verification result,
                                       mismatch info, client extension methods
                                       (no upstream counterpart).
    CodeBrix.SSH.Security              Host key algorithms, key exchange
                                       algorithms, Certificate (with its
                                       nested CertificateType enum),
                                       SshKeyData.
    CodeBrix.SSH.Security.Cryptography Key and digital-signature
                                       implementations (RsaKey, EcdsaKey,
                                       ED25519Key and their signatures).
    CodeBrix.SSH.Security.Cryptography.Ciphers
                                       Cipher implementations (AesCipher,
                                       AesGcmCipher, ChaCha20Poly1305Cipher,
                                       TripleDesCipher) and .Modes.
    CodeBrix.SSH.Messages              SSH wire protocol messages, split into
    CodeBrix.SSH.Messages.Authentication  .Authentication, .Connection and
    CodeBrix.SSH.Messages.Connection      .Transport. Needed only when
    CodeBrix.SSH.Messages.Transport        inspecting DisconnectReason or
                                       writing a custom RequestInfo.
    CodeBrix.SSH.Compression           Compressor, Zlib, ZlibOpenSsh.
    CodeBrix.SSH.Connection            SshIdentification (the server banner
                                       reported by ServerIdentificationReceived).
    CodeBrix.SSH.NetConf               NETCONF-over-SSH subsystem session.
    CodeBrix.SSH.Channels              SSH channel implementations. No public
    CodeBrix.SSH.Abstractions          types; do not add these usings.

Almost all consumer code needs only the first four.

MIGRATING FROM SSH.NET: replace "Renci.SshNet" with "CodeBrix.SSH" in every
using directive and fully-qualified name. Nothing else should need to change.


CORE API REFERENCE
================================================================================

THE CLIENT FAMILY (BaseClient)
--------------------------------------------------------------------------------

SshClient, SftpClient, ScpClient and NetConfClient all derive from the
abstract BaseClient, which implements IBaseClient : IDisposable and owns the
connection lifecycle. Everything in this sub-section is available on all four
clients.

    public abstract class BaseClient : IBaseClient
    {
        public ConnectionInfo ConnectionInfo { get; }
        public virtual bool IsConnected { get; }
        public TimeSpan KeepAliveInterval { get; set; }

        public event EventHandler<ExceptionEventArgs> ErrorOccurred;
        public event EventHandler<HostKeyEventArgs> HostKeyReceived;
        public event EventHandler<SshIdentificationEventArgs> ServerIdentificationReceived;

        public void Connect();
        public Task ConnectAsync(CancellationToken cancellationToken);
        public void Disconnect();
        public void SendKeepAlive();
        public void Dispose();
    }

  * KeepAliveInterval sends periodic keep-alive messages on an otherwise idle
    connection. The default is Timeout.InfiniteTimeSpan (-1 milliseconds),
    which disables it. Set it BEFORE Connect(); 30 seconds is a reasonable
    value for a long-lived interactive session.
  * ErrorOccurred surfaces asynchronous errors raised on the session's
    background threads -- errors that have no call to throw out of.
  * HostKeyReceived fires once per connection, during the key exchange, before
    authentication. See HOST KEY VERIFICATION below.
  * ServerIdentificationReceived carries the server's version banner as an
    SshIdentification (ProtocolVersion, SoftwareVersion, Comments).
  * Every client is IDisposable and Dispose() disconnects. Always use `using`.

Each concrete client offers the same five constructor shapes:

    new XClient(ConnectionInfo connectionInfo)
    new XClient(string host, string username, string password)
    new XClient(string host, int port, string username, string password)
    new XClient(string host, string username, params IPrivateKeySource[] keyFiles)
    new XClient(string host, int port, string username, params IPrivateKeySource[] keyFiles)

The convenience constructors build a PasswordConnectionInfo or a
PrivateKeyConnectionInfo internally and own it, so it is disposed with the
client. Pass a ConnectionInfo yourself whenever you need multi-factor
authentication, a proxy, altered timeouts, or a restricted algorithm set --
see AUTHENTICATION AND CONNECTIONINFO.


SshClient
--------------------------------------------------------------------------------

    public class SshClient : BaseClient, ISshClient
    {
        public IEnumerable<ForwardedPort> ForwardedPorts { get; }

        public SshCommand CreateCommand(string commandText);
        public SshCommand CreateCommand(string commandText, Encoding encoding);
        public SshCommand RunCommand(string commandText);

        public void AddForwardedPort(ForwardedPort port);
        public void RemoveForwardedPort(ForwardedPort port);

        public ShellStream CreateShellStream(string terminalName, uint columns,
            uint rows, uint width, uint height, int bufferSize);
        public ShellStream CreateShellStream(string terminalName, uint columns,
            uint rows, uint width, uint height, int bufferSize,
            IDictionary<TerminalModes, uint> terminalModeValues);
        public ShellStream CreateShellStreamNoTerminal(int bufferSize = -1);

        public Shell CreateShell(Stream input, Stream output, Stream extendedOutput);
        public Shell CreateShell(Stream input, Stream output, Stream extendedOutput,
            string terminalName, uint columns, uint rows, uint width, uint height,
            IDictionary<TerminalModes, uint> terminalModes);
        public Shell CreateShell(Stream input, Stream output, Stream extendedOutput,
            string terminalName, uint columns, uint rows, uint width, uint height,
            IDictionary<TerminalModes, uint> terminalModes, int bufferSize);
        public Shell CreateShell(Encoding encoding, string input, Stream output,
            Stream extendedOutput);
        public Shell CreateShell(Encoding encoding, string input, Stream output,
            Stream extendedOutput, string terminalName, uint columns, uint rows,
            uint width, uint height, IDictionary<TerminalModes, uint> terminalModes);
        public Shell CreateShell(Encoding encoding, string input, Stream output,
            Stream extendedOutput, string terminalName, uint columns, uint rows,
            uint width, uint height, IDictionary<TerminalModes, uint> terminalModes,
            int bufferSize);
        public Shell CreateShellNoTerminal(Stream input, Stream output,
            Stream extendedOutput, int bufferSize = -1);
    }

    using var client = new SshClient("host", "user", "password");
    client.Connect();

    using SshCommand cmd = client.RunCommand("uname -a");
    Console.WriteLine(cmd.Result);
    Console.WriteLine(cmd.ExitStatus);   // int?

  * RunCommand(text) is CreateCommand(text) + Execute(); it returns the
    completed SshCommand, which the caller must dispose.
  * CreateCommand(text, encoding) also assigns the encoding to
    ConnectionInfo.Encoding, so it affects subsequent commands on the same
    client. Set ConnectionInfo.Encoding once instead if you want one encoding
    everywhere.
  * Each command opens a fresh exec channel on the already-authenticated
    connection, so it is safe to call RunCommand while an interactive
    ShellStream is live on the same client: probing commands and an open
    terminal multiplex on separate channels without interfering with each
    other in either direction. ConnectionInfo.MaxSessions (default 10) caps
    how many session channels may be open simultaneously; the eleventh
    concurrent command or shell blocks until one closes.
  * CreateShellStream requires a live connection; terminalName is passed
    through verbatim and becomes TERM on the remote side. bufferSize is the
    INITIAL size of the internal read and write buffers, which grow on demand;
    -1 selects 1024 bytes.
  * The *NoTerminal variants request a shell without allocating a
    pseudo-terminal, equivalent to `ssh -T`.
  * CreateShell wires an existing input Stream (or a string plus Encoding) to
    output and extendedOutput streams; the returned Shell must be Start()ed.
    ShellStream is the better choice for anything interactive.
  * AddForwardedPort requires the client to be CONNECTED; it throws if the
    session is not open. RemoveForwardedPort stops the port first.


SshCommand
--------------------------------------------------------------------------------

    public sealed class SshCommand : IDisposable
    {
        public string CommandText { get; }
        public TimeSpan CommandTimeout { get; set; }   // default: infinite

        public int? ExitStatus { get; }                // null until reported
        public string ExitSignal { get; }              // e.g. "TERM", "KILL"

        public Stream OutputStream { get; }            // stdout, a PipeStream
        public Stream ExtendedOutputStream { get; }    // stderr
        public Stream CreateInputStream();             // stdin

        public string Result { get; }                  // reads OutputStream
        public string Error { get; }                   // reads ExtendedOutputStream

        public string Execute();
        public string Execute(string commandText);
        public Task ExecuteAsync(CancellationToken cancellationToken = default);

        public IAsyncResult BeginExecute();
        public IAsyncResult BeginExecute(AsyncCallback callback);
        public IAsyncResult BeginExecute(AsyncCallback callback, object state);
        public IAsyncResult BeginExecute(string commandText, AsyncCallback callback,
                                         object state);
        public string EndExecute(IAsyncResult asyncResult);

        public void CancelAsync(bool forceKill = false, int millisecondsTimeout = 500);
        public void Dispose();
    }

  * Result and Error are lazy: the first read drains the corresponding stream
    to the end and caches the string. Reading Result yourself and then reading
    OutputStream (or vice versa) gets you nothing the second time.
  * Error returns string.Empty unless the server actually sent extended data
    marked as stderr.
  * ExitStatus is null when the server reported no exit status; a command
    killed by a signal reports ExitSignal instead (ABRT, ALRM, FPE, HUP, ILL,
    INT, KILL, PIPE, QUIT, SEGV, TER, USR1, USR2 per RFC 4254 6.10).
  * CommandTimeout defaults to Timeout.InfiniteTimeSpan. When set, exceeding
    it throws SshOperationTimeoutException from the execute call.
  * ExecuteAsync's cancellation token does not abandon the command locally --
    it sends a TERM signal to the remote process, then completes the task as
    cancelled. CancelAsync does the same thing explicitly, with forceKill
    selecting KILL over TERM.
  * BeginExecute / EndExecute are the APM wrappers over ExecuteAsync;
    EndExecute returns Result. Calling EndExecute with an IAsyncResult from a
    different instance or from a previous invocation throws ArgumentException.
  * CreateInputStream is valid only while the command is executing (that is,
    after ExecuteAsync/BeginExecute has been started and before it completes),
    and only once per command. DISPOSE the returned stream to signal EOF to
    the remote process -- forgetting to do so can leave the command running
    forever.
  * SshCommand is IDisposable and holds a channel; always dispose it.

Streaming a long-running command instead of buffering it:

    using SshCommand cmd = client.CreateCommand("tail -n 200 -f /var/log/syslog");
    Task running = cmd.ExecuteAsync(cancellationToken);

    using (var reader = new StreamReader(cmd.OutputStream, Encoding.UTF8))
    {
        string line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            Console.WriteLine(line);
        }
    }

    await running;


AUTHENTICATION AND CONNECTIONINFO
--------------------------------------------------------------------------------

ConnectionInfo is the full-control alternative to the convenience client
constructors. It carries the host, port, username, proxy settings, timeouts,
encoding, the negotiable algorithm sets, and the ordered list of
authentication methods to try.

    public class ConnectionInfo
    {
        public ConnectionInfo(string host, string username,
                              params AuthenticationMethod[] authenticationMethods);
        public ConnectionInfo(string host, int port, string username,
                              params AuthenticationMethod[] authenticationMethods);
        public ConnectionInfo(string host, int port, string username,
                              ProxyTypes proxyType, string proxyHost, int proxyPort,
                              string proxyUsername, string proxyPassword,
                              params AuthenticationMethod[] authenticationMethods);

        public string Host { get; }
        public int Port { get; }                       // 22 in the short overloads
        public string Username { get; }
        public bool IsAuthenticated { get; }

        public ProxyTypes ProxyType { get; }
        public string ProxyHost { get; }
        public int ProxyPort { get; }
        public string ProxyUsername { get; }
        public string ProxyPassword { get; }

        public TimeSpan Timeout { get; set; }              // default 30 seconds
        public TimeSpan ChannelCloseTimeout { get; set; }  // default 1 second
        public Encoding Encoding { get; set; }             // default UTF8
        public int RetryAttempts { get; set; }             // default 10
        public int MaxSessions { get; set; }               // default 10
        public ILoggerFactory LoggerFactory { get; set; }

        public IList<AuthenticationMethod> AuthenticationMethods { get; }
        public IOrderedDictionary<string, Func<IKeyExchange>> KeyExchangeAlgorithms { get; }
        public IOrderedDictionary<string, CipherInfo> Encryptions { get; }
        public IOrderedDictionary<string, HashInfo> HmacAlgorithms { get; }
        public IOrderedDictionary<string, Func<byte[], KeyHostAlgorithm>> HostKeyAlgorithms { get; }
        public IOrderedDictionary<string, Func<Compressor>> CompressionAlgorithms { get; }
        public IDictionary<string, RequestInfo> ChannelRequests { get; }

        public event EventHandler<AuthenticationBannerEventArgs> AuthenticationBanner;

        // Populated during/after key exchange:
        public string ServerVersion { get; }
        public string ClientVersion { get; }
        public string CurrentKeyExchangeAlgorithm { get; }
        public string CurrentHostKeyAlgorithm { get; }
        public string CurrentClientEncryption { get; }
        public string CurrentServerEncryption { get; }
        public string CurrentClientHmacAlgorithm { get; }
        public string CurrentServerHmacAlgorithm { get; }
        public string CurrentClientCompressionAlgorithm { get; }
        public string CurrentServerCompressionAlgorithm { get; }
    }

The algorithm dictionaries are ordered and mutable: remove an entry to refuse
that algorithm, or re-add it to change preference order. They are populated
with the library's defaults by the constructor, so mutate them AFTER
constructing the ConnectionInfo and BEFORE connecting:

    var info = new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "pw"));
    _ = info.Encryptions.Remove("3des-cbc");
    _ = info.HmacAlgorithms.Remove("hmac-sha1");
    _ = info.KeyExchangeAlgorithms.Remove("diffie-hellman-group1-sha1");

ProxyTypes: None, Socks4, Socks5, Http. A proxied connection uses the
eight-plus-params constructor:

    var info = new ConnectionInfo(
        "ssh.internal", 22, "user",
        ProxyTypes.Socks5, "proxy.example.com", 1080, "proxyuser", "proxypass",
        new PasswordAuthenticationMethod("user", "pw"));

    using var client = new SshClient(info);

AUTHENTICATION METHODS. All derive from the abstract AuthenticationMethod,
which is IDisposable and exposes Name, Username, AllowedAuthentications and
Authenticate(Session) returning AuthenticationResult (Success, PartialSuccess,
Failure). You supply instances; the library calls them.

    public class PasswordAuthenticationMethod : AuthenticationMethod
    {
        public PasswordAuthenticationMethod(string username, string password);
        public PasswordAuthenticationMethod(string username, byte[] password);
        public event EventHandler<AuthenticationPasswordChangeEventArgs> PasswordExpired;
    }

    public class PrivateKeyAuthenticationMethod : AuthenticationMethod
    {
        public PrivateKeyAuthenticationMethod(string username,
                                              params IPrivateKeySource[] keyFiles);
        public ICollection<IPrivateKeySource> KeyFiles { get; }
    }

    public class KeyboardInteractiveAuthenticationMethod : AuthenticationMethod
    {
        public KeyboardInteractiveAuthenticationMethod(string username);
        public event EventHandler<AuthenticationPromptEventArgs> AuthenticationPrompt;
    }

    public class NoneAuthenticationMethod : AuthenticationMethod
    {
        public NoneAuthenticationMethod(string username);
    }

  * PasswordExpired fires when the server demands a password change; set
    AuthenticationPasswordChangeEventArgs.NewPassword (a byte[]) to supply the
    replacement.
  * NoneAuthenticationMethod is chiefly a probe: servers answer it with the
    list of authentications they will accept, which lands in
    AllowedAuthentications.
  * Passing several methods to ConnectionInfo drives MULTI-FACTOR
    authentication. The library first attempts the `none` method, which is how
    the server's list of allowed authentications is discovered (and which
    succeeds outright on a server that requires no authentication). It then
    tries YOUR methods IN THE ORDER YOU SUPPLIED THEM, filtered to the ones
    the server currently allows -- your order wins over the server's. A
    PartialSuccess result re-reads the server's updated allowed list and
    recurses, which is how a password-then-one-time-code chain completes. A
    method that returns PartialSuccess five times is dropped, guarding against
    servers that never update their list. Exhausting the methods throws
    SshAuthenticationException ("No suitable authentication method found ..."
    or "Permission denied (<method>)").

KEYBOARD-INTERACTIVE. The event hands you an
AuthenticationPromptEventArgs and you must fill in a Response for EVERY
prompt:

    public class AuthenticationPromptEventArgs : AuthenticationEventArgs
    {
        public string Username { get; }        // from AuthenticationEventArgs
        public string Instruction { get; }
        public string Language { get; }
        public IReadOnlyList<AuthenticationPrompt> Prompts { get; }
    }

    public class AuthenticationPrompt
    {
        public int Id { get; }
        public bool IsEchoed { get; }          // false for passwords
        public string Request { get; }         // e.g. "Password: "
        public string Response { get; set; }   // YOU must set this
    }

Leaving any Response null throws SshAuthenticationException naming the prompt.
Prompts are answered in ascending Id order regardless of list order.

    var keyboard = new KeyboardInteractiveAuthenticationMethod("user");
    keyboard.AuthenticationPrompt += (sender, e) =>
    {
        foreach (AuthenticationPrompt prompt in e.Prompts)
        {
            prompt.Response = prompt.Request.Contains("Verification code")
                ? ReadOneTimeCode()
                : password;
        }
    };

CONVENIENCE CONNECTIONINFO SUBCLASSES. PasswordConnectionInfo,
PrivateKeyConnectionInfo and KeyboardInteractiveConnectionInfo each wrap a
single method and add proxy-carrying constructor overloads. All three are
IDisposable, and all three re-expose the corresponding event
(PasswordConnectionInfo.PasswordExpired,
KeyboardInteractiveConnectionInfo.AuthenticationPrompt) so you can subscribe
without building the method object yourself:

    public class PasswordConnectionInfo : ConnectionInfo, IDisposable
    {
        public PasswordConnectionInfo(string host, string username, string password);
        public PasswordConnectionInfo(string host, int port, string username, string password);
        public PasswordConnectionInfo(string host, int port, string username, string password,
            ProxyTypes proxyType, string proxyHost, int proxyPort);
        // ... plus byte[]-password and proxy-credential overloads
        public event EventHandler<AuthenticationPasswordChangeEventArgs> PasswordExpired;
    }

    public class PrivateKeyConnectionInfo : ConnectionInfo, IDisposable
    {
        public PrivateKeyConnectionInfo(string host, string username,
                                        params IPrivateKeySource[] keyFiles);
        public PrivateKeyConnectionInfo(string host, int port, string username,
                                        params IPrivateKeySource[] keyFiles);
        public ICollection<IPrivateKeySource> KeyFiles { get; }
        // ... plus proxy overloads
    }

    public class KeyboardInteractiveConnectionInfo : ConnectionInfo, IDisposable
    {
        public KeyboardInteractiveConnectionInfo(string host, string username);
        public KeyboardInteractiveConnectionInfo(string host, int port, string username);
        public event EventHandler<AuthenticationPromptEventArgs> AuthenticationPrompt;
        // ... plus proxy overloads
    }


PRIVATE KEYS AND CERTIFICATES
--------------------------------------------------------------------------------

    public partial class PrivateKeyFile : IPrivateKeySource, IDisposable
    {
        public PrivateKeyFile(string fileName);
        public PrivateKeyFile(string fileName, string passPhrase);
        public PrivateKeyFile(string fileName, string passPhrase, string certificateFileName);
        public PrivateKeyFile(Stream privateKey);
        public PrivateKeyFile(Stream privateKey, string passPhrase);
        public PrivateKeyFile(Stream privateKey, string passPhrase, Stream certificate);
        public PrivateKeyFile(Key key);

        public Key Key { get; }
        public Certificate Certificate { get; }
        public IReadOnlyCollection<HostAlgorithm> HostKeyAlgorithms { get; }
    }

    var key = new PrivateKeyFile("/home/me/.ssh/id_ed25519", "passphrase");
    using var client = new SshClient("host", "user", key);

Supported key types are RSA, ECDSA (nistp256/384/521) and ED25519, read from
OpenSSL traditional PEM (PKCS#1), PKCS#8, ssh.com, OpenSSH and PuTTY v2/v3
files. An encrypted key with a null or empty passphrase throws
SshPassPhraseNullOrEmptyException.

Passing a certificate file (or stream) alongside the key enables OpenSSH
CERTIFICATE authentication: the client then offers the
"*-cert-v01@openssh.com" host key algorithms. The parsed certificate is
exposed as:

    public class Certificate
    {
        public Certificate(byte[] data);
        public string Name { get; }                  // e.g. ssh-ed25519-cert-v01@openssh.com
        public Key Key { get; }
        public ulong Serial { get; }
        public CertificateType Type { get; }         // nested enum
                                                     // Certificate.CertificateType:
                                                     // User = 1, Host = 2
        public string KeyId { get; }
        public IList<string> ValidPrincipals { get; }
        public DateTimeOffset ValidAfter { get; }
        public DateTimeOffset ValidBefore { get; }
        public ulong ValidAfterUnixSeconds { get; }
        public ulong ValidBeforeUnixSeconds { get; }
        public IDictionary<string, string> CriticalOptions { get; }
        public IDictionary<string, string> Extensions { get; }
        public byte[] CertificateAuthorityKey { get; }
        public string CertificateAuthorityKeyFingerPrint { get; }
        public byte[] Signature { get; }
        public byte[] Nonce { get; }
    }

The host-key side of the same machinery is HostAlgorithm (abstract: Name,
Data, Sign, VerifySignature), KeyHostAlgorithm (Key, DigitalSignature) and
CertificateHostAlgorithm (adds Certificate). Key implementations live in
CodeBrix.SSH.Security.Cryptography: RsaKey, EcdsaKey, ED25519Key, with
RsaDigitalSignature, EcdsaDigitalSignature and ED25519DigitalSignature. Most
consumers never touch these directly -- they appear in
ConnectionInfo.HostKeyAlgorithms factories and in HostKeyEventArgs.


HOST KEY VERIFICATION
--------------------------------------------------------------------------------

CRITICAL DEFAULT: HostKeyEventArgs.CanTrust is initialised to TRUE, so with no
HostKeyReceived subscriber the client accepts ANY host key. That is a
man-in-the-middle exposure. Decide a policy deliberately.

    public class HostKeyEventArgs : EventArgs
    {
        public bool CanTrust { get; set; }        // starts true
        public byte[] HostKey { get; }            // the key blob
        public string HostKeyName { get; }        // e.g. "ssh-ed25519"
        public int KeyLength { get; }             // bits
        public Certificate Certificate { get; }   // null if no certificate
        public byte[] FingerPrint { get; }        // raw MD5 bytes
        public string FingerPrintSHA256 { get; }  // base64, no padding, no "SHA256:" prefix
        public string FingerPrintMD5 { get; }     // colon-separated hex, no "MD5:" prefix
    }

Accept-all is one line, and is acceptable only for a throwaway dev tool:

    client.HostKeyReceived += (sender, e) => e.CanTrust = true;

Pinning a single known fingerprint:

    client.HostKeyReceived += (sender, e) =>
        e.CanTrust = e.FingerPrintSHA256 == "ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og";

KNOWN_HOSTS (CodeBrix.SSH.KnownHosts -- no upstream counterpart). For the
ground between "verify nothing" and hand-written logic, this namespace
provides a store and two ready-made policies, exposed as extension methods on
any client (SSH, SFTP, SCP, NETCONF -- they extend BaseClient):

    public static class KnownHostsClientExtensions
    {
        public static void UseStrictHostKeyVerification(this BaseClient client,
                                                        KnownHostsStore store);
        public static void UseTrustOnFirstUse(this BaseClient client,
                                              KnownHostsStore store);
        public static void UseTrustOnFirstUse(this BaseClient client,
                                              KnownHostsStore store,
                                              Func<HostKeyMismatchInfo, bool> mismatchCallback);
    }

    public sealed class KnownHostsStore
    {
        public static string DefaultFilePath { get; }
        public KnownHostsStore(string filePath);
        public string FilePath { get; }
        public HostKeyVerificationResult Verify(string host, int port,
                                                string keyAlgorithm, byte[] key);
        public void Add(string host, int port, string keyAlgorithm, byte[] key);
        public void Save();
    }

    public enum HostKeyVerificationResult { Unknown = 0, Known = 1, Mismatch = 2, Revoked = 3 }

    public sealed class HostKeyMismatchInfo
    {
        public string Host { get; }
        public int Port { get; }
        public string KeyAlgorithm { get; }
        public byte[] HostKey { get; }
        public string FingerPrintSHA256 { get; }
        public HostKeyVerificationResult Result { get; }
    }

    using CodeBrix.SSH.KnownHosts;

    var store = new KnownHostsStore(KnownHostsStore.DefaultFilePath);

    client.UseStrictHostKeyVerification(store);   // known keys only
    client.UseTrustOnFirstUse(store);             // TOFU

    client.UseTrustOnFirstUse(store, mismatch =>
    {
        // Changed or revoked key -- warn, return true to proceed anyway.
        ShowHostKeyWarning(mismatch.Host, mismatch.Port,
                           mismatch.FingerPrintSHA256, mismatch.Result);
        return false;
    });

Call ONE policy method per client, after construction and before Connect().
Each call adds a HostKeyReceived handler, so calling twice stacks handlers and
the most restrictive outcome wins (any handler can veto trust).

  * Strict trusts only keys already in the store and never writes to it;
    Unknown, Mismatch and Revoked are all rejected.
  * Trust-on-first-use trusts known keys, trusts and records unknown keys
    (calling Save() immediately), and rejects Mismatch or Revoked -- or defers
    those to the callback overload, whose return value decides. Accepting a
    mismatch never modifies the store.
  * KnownHostsStore reads and writes the OpenSSH known_hosts format: plain and
    [host]:port entries, comma-separated host lists, * and ? wildcards, !
    negation, hashed |1| host lines and the @revoked marker are all honoured
    when reading. @cert-authority lines are IGNORED -- host certificates are
    out of scope, and a certificate blob is treated as an ordinary key of its
    certificate algorithm. Entries appended by Add are plain-hostname lines.
  * DefaultFilePath resolves ~/.ssh/known_hosts on Linux and macOS and
    %USERPROFILE%\.ssh\known_hosts on Windows (where the bundled OpenSSH
    client uses the same location). Point the store there, or at an
    application-specific file, which Save() creates on demand with owner-only
    permissions on Linux and macOS.
  * A missing file is not an error -- it is an empty store.
  * Verify, Add and Save are thread-safe; both LF and CRLF files are read, and
    LF is written.


SFTP (SftpClient)
--------------------------------------------------------------------------------

    public class SftpClient : BaseClient, ISftpClient
    {
        public uint BufferSize { get; set; }           // default 32768 (32 KB)
        public TimeSpan OperationTimeout { get; set; } // default infinite
        public string WorkingDirectory { get; }
        public int ProtocolVersion { get; }
        public override bool IsConnected { get; }      // also requires an open SFTP session
    }

Method families, all on SftpClient (and on ISftpClient):

  Navigation and metadata
        void ChangeDirectory(string path);
        Task ChangeDirectoryAsync(string path, CancellationToken ct = default);
        ISftpFile Get(string path);
        Task<ISftpFile> GetAsync(string path, CancellationToken ct);
        bool Exists(string path);
        Task<bool> ExistsAsync(string path, CancellationToken ct = default);
        SftpFileAttributes GetAttributes(string path);
        Task<SftpFileAttributes> GetAttributesAsync(string path, CancellationToken ct);
        void SetAttributes(string path, SftpFileAttributes fileAttributes);
        void ChangePermissions(string path, short mode);          // e.g. 0644 -> 420
        DateTime GetLastAccessTime(string path);
        DateTime GetLastAccessTimeUtc(string path);
        DateTime GetLastWriteTime(string path);
        DateTime GetLastWriteTimeUtc(string path);
        void SetLastAccessTime(string path, DateTime lastAccessTime);
        void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc);
        void SetLastWriteTime(string path, DateTime lastWriteTime);
        void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);
        SftpFileSystemInformation GetStatus(string path);
        Task<SftpFileSystemInformation> GetStatusAsync(string path, CancellationToken ct);

  Directory listing
        IEnumerable<ISftpFile> ListDirectory(string path, Action<int> listCallback = null);
        IAsyncEnumerable<ISftpFile> ListDirectoryAsync(string path, CancellationToken ct);
        IAsyncResult BeginListDirectory(string path, AsyncCallback asyncCallback,
                                        object state, Action<int> listCallback = null);
        IEnumerable<ISftpFile> EndListDirectory(IAsyncResult asyncResult);

  Create, rename, delete, link
        void CreateDirectory(string path);
        Task CreateDirectoryAsync(string path, CancellationToken ct = default);
        void DeleteDirectory(string path);
        Task DeleteDirectoryAsync(string path, CancellationToken ct = default);
        void DeleteFile(string path);
        Task DeleteFileAsync(string path, CancellationToken ct);
        void Delete(string path);                 // file or directory
        Task DeleteAsync(string path, CancellationToken ct = default);
        void RenameFile(string oldPath, string newPath);
        void RenameFile(string oldPath, string newPath, bool isPosix);
        Task RenameFileAsync(string oldPath, string newPath, CancellationToken ct);
        void SymbolicLink(string path, string linkPath);

  Whole-file transfer
        void UploadFile(Stream input, string path, Action<ulong> uploadCallback = null);
        void UploadFile(Stream input, string path, bool canOverride,
                        Action<ulong> uploadCallback = null);
        Task UploadFileAsync(Stream input, string path, CancellationToken ct = default);
        Task UploadFileAsync(Stream input, string path,
                             IProgress<UploadFileProgressReport> uploadProgress,
                             CancellationToken ct = default);
        Task UploadFileAsync(Stream input, string path, bool canOverride,
                             IProgress<UploadFileProgressReport> uploadProgress = null,
                             CancellationToken ct = default);
        void DownloadFile(string path, Stream output, Action<ulong> downloadCallback = null);
        Task DownloadFileAsync(string path, Stream output, CancellationToken ct = default);
        Task DownloadFileAsync(string path, Stream output,
                               IProgress<DownloadFileProgressReport> downloadProgress,
                               CancellationToken ct = default);
        IAsyncResult BeginUploadFile(...);   // four overloads, incl. canOverride
        void EndUploadFile(IAsyncResult asyncResult);
        IAsyncResult BeginDownloadFile(...); // three overloads
        void EndDownloadFile(IAsyncResult asyncResult);

  Streams and text helpers (System.IO.File-shaped)
        SftpFileStream Open(string path, FileMode mode);
        SftpFileStream Open(string path, FileMode mode, FileAccess access);
        Task<SftpFileStream> OpenAsync(string path, FileMode mode, FileAccess access,
                                       CancellationToken ct);
        SftpFileStream OpenRead(string path);
        SftpFileStream OpenWrite(string path);
        SftpFileStream Create(string path);
        SftpFileStream Create(string path, int bufferSize);
        StreamReader OpenText(string path);
        StreamWriter CreateText(string path);
        StreamWriter CreateText(string path, Encoding encoding);
        StreamWriter AppendText(string path);
        StreamWriter AppendText(string path, Encoding encoding);
        byte[] ReadAllBytes(string path);
        string ReadAllText(string path);
        string ReadAllText(string path, Encoding encoding);
        string[] ReadAllLines(string path);
        string[] ReadAllLines(string path, Encoding encoding);
        IEnumerable<string> ReadLines(string path);
        IEnumerable<string> ReadLines(string path, Encoding encoding);
        void WriteAllBytes(string path, byte[] bytes);
        void WriteAllText(string path, string contents);
        void WriteAllText(string path, string contents, Encoding encoding);
        void WriteAllLines(string path, IEnumerable<string> contents);
        void WriteAllLines(string path, string[] contents);
        void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding);
        void WriteAllLines(string path, string[] contents, Encoding encoding);
        void AppendAllText(string path, string contents);
        void AppendAllText(string path, string contents, Encoding encoding);
        void AppendAllLines(string path, IEnumerable<string> contents);
        void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding);

  Directory synchronization
        IEnumerable<FileInfo> SynchronizeDirectories(string sourcePath,
                                                     string destinationPath,
                                                     string searchPattern);
        IAsyncResult BeginSynchronizeDirectories(string sourcePath, string destinationPath,
                                                 string searchPattern,
                                                 AsyncCallback asyncCallback, object state);
        IEnumerable<FileInfo> EndSynchronizeDirectories(IAsyncResult asyncResult);

Notes that matter:

  * UploadFile / UploadFileAsync default to canOverride: true (truncate an
    existing file). With canOverride: false the SFTP open uses CREATE_NEW, so
    an existing file makes the operation fail.
  * The canOverride overload takes uploadProgress BEFORE the cancellation
    token, so pass the token by NAME when you omit the progress:
    UploadFileAsync(stream, path, canOverride: false, cancellationToken: token).
  * SynchronizeDirectories is one-way (local -> remote), NON-recursive (only
    files matching searchPattern directly in sourcePath), and compares by FILE
    SIZE only -- same size means "not different", so same-size edits are not
    re-uploaded. It returns the FileInfo of each file it uploaded. A missing
    local source directory throws FileNotFoundException; an upload failure
    throws SshException wrapping the cause.
  * ListDirectoryAsync's CancellationToken parameter is REQUIRED (no default).
  * The listCallback of ListDirectory / BeginListDirectory receives the running
    count of entries read so far.
  * Get(path) throws SftpPathNotFoundException when the path does not exist;
    Exists(path) is the non-throwing test.

SFTP DATA TYPES (CodeBrix.SSH.Sftp)

    public interface ISftpFile            // implemented by SftpFile
    {
        SftpFileAttributes Attributes { get; }
        string FullName { get; }
        string Name { get; }
        long Length { get; }
        DateTime LastAccessTime { get; set; }
        DateTime LastWriteTime { get; set; }
        DateTime LastAccessTimeUtc { get; set; }
        DateTime LastWriteTimeUtc { get; set; }
        int UserId { get; set; }
        int GroupId { get; set; }
        bool IsDirectory { get; }
        bool IsRegularFile { get; }
        bool IsSymbolicLink { get; }
        bool IsSocket { get; }
        bool IsBlockDevice { get; }
        bool IsCharacterDevice { get; }
        bool IsNamedPipe { get; }
        bool OwnerCanRead { get; set; }   // ... OwnerCanWrite/Execute,
        bool GroupCanRead { get; set; }   //     GroupCan*, OthersCan*
        bool OthersCanRead { get; set; }
        void SetPermissions(short mode);
        void Delete();
        Task DeleteAsync(CancellationToken cancellationToken = default);
        void MoveTo(string destFileName);
        void UpdateStatus();
    }

  * The setters on ISftpFile mutate the in-memory attributes only; call
    UpdateStatus() to push them to the server (and to re-read them).

    public sealed class SftpFileAttributes
    {
        public long Size { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        public bool IsDirectory { get; }        // ... IsRegularFile, IsSymbolicLink,
                                                //     IsSocket, IsBlockDevice,
                                                //     IsCharacterDevice, IsNamedPipe
        public bool IsUIDBitSet { get; set; }
        public bool IsGroupIDBitSet { get; set; }
        public bool IsStickyBitSet { get; set; }
        public bool OwnerCanRead { get; set; }  // ... nine permission bits in total
        public IDictionary<string, string> Extensions { get; }
        public void SetPermissions(short mode);
        public byte[] GetBytes();
        public override string ToString();      // "drwxr-xr-x Size: N
                                                //  LastWriteTime: ..." summary
    }

    public sealed class SftpFileSystemInformation   // from GetStatus / GetStatusAsync
    {
        public ulong FileSystemBlockSize { get; }
        public ulong BlockSize { get; }
        public ulong TotalBlocks { get; }
        public ulong FreeBlocks { get; }
        public ulong AvailableBlocks { get; }
        public ulong TotalNodes { get; }
        public ulong FreeNodes { get; }
        public ulong AvailableNodes { get; }
        public ulong Sid { get; }
        public bool IsReadOnly { get; }
        public bool SupportsSetUid { get; }
        public ulong MaxNameLenght { get; }     // note the upstream spelling
    }

    public sealed partial class SftpFileStream : Stream
    {
        public string Name { get; }
        public byte[] Handle { get; }
        public TimeSpan Timeout { get; set; }
        public override bool CanRead / CanSeek / CanWrite / CanTimeout { get; }
        public override long Length { get; }
        public override long Position { get; set; }
        // full sync + async Stream surface: Read/ReadAsync/ReadByte,
        // Write/WriteAsync/WriteByte, Seek, SetLength, Flush/FlushAsync,
        // BeginRead/EndRead, BeginWrite/EndWrite, Dispose/DisposeAsync
    }

SftpFileStream is a real seekable stream over a remote file, so it composes
with StreamReader, StreamWriter, JsonSerializer, CopyToAsync and anything else
that takes a Stream.

    public enum StatusCode { ... }   // SFTP status codes; surfaced on SftpException

Progress reporting on the async transfer methods:

    var progress = new Progress<DownloadFileProgressReport>(
        r => Console.WriteLine(r.TotalBytesDownloaded));

    await client.DownloadFileAsync("/remote/file", stream, progress, cancellationToken);

    public struct DownloadFileProgressReport { public ulong TotalBytesDownloaded { get; } }
    public struct UploadFileProgressReport   { public ulong TotalBytesUploaded { get; } }

The synchronous and APM overloads take an Action<ulong> callback instead; that
callback is invoked on the thread pool, not on the caller's synchronization
context.


SCP (ScpClient)
--------------------------------------------------------------------------------

    public partial class ScpClient : BaseClient
    {
        public uint BufferSize { get; set; }                 // default 16384 (16 KB)
        public TimeSpan OperationTimeout { get; set; }       // default infinite
        public IRemotePathTransformation RemotePathTransformation { get; set; }
        public bool UseDirectoryFlag { get; set; } = true;

        public event EventHandler<ScpUploadEventArgs> Uploading;
        public event EventHandler<ScpDownloadEventArgs> Downloading;

        public void Upload(Stream source, string path);
        public void Upload(FileInfo fileInfo, string path);
        public void Upload(DirectoryInfo directoryInfo, string path);
        public void Download(string filename, Stream destination);
        public void Download(string filename, FileInfo fileInfo);
        public void Download(string directoryName, DirectoryInfo directoryInfo);
    }

    public class ScpUploadEventArgs   : EventArgs { string Filename; long Size; long Uploaded; }
    public class ScpDownloadEventArgs : EventArgs { string Filename; long Size; long Downloaded; }

  * SCP is synchronous only; there are no async overloads. Use SFTP when you
    need cancellation or IProgress.
  * UseDirectoryFlag controls the undocumented "-d" flag on the remote scp
    command line. Leave it true; set it false for servers (Cisco IOS and
    similar) whose scp rejects the flag.
  * RemotePathTransformation defaults to RemotePathTransformation.DoubleQuote.
    The remote path is interpolated into a shell command line on the server,
    so the transformation is what keeps spaces and metacharacters safe. The
    built-ins are, from CodeBrix.SSH.RemotePathTransformation:
        DoubleQuote  -- wraps in "..." and backslash-escapes embedded quotes.
        ShellQuote   -- single-quote-based quoting that also escapes "!" for
                        C shell; the safest general choice.
        None         -- no transformation; only for servers that need none.
    IRemotePathTransformation has a single member, string Transform(string
    path), so a custom rule is a small class.
  * Download(string, FileInfo) truncates the local file (a fork fix; upstream
    left stale trailing bytes when overwriting a larger file).


INTERACTIVE SHELL: ShellStream AND Shell
--------------------------------------------------------------------------------

ShellStream is a Stream over an interactive shell session, obtained from
SshClient.CreateShellStream / CreateShellStreamNoTerminal. It serves two
distinct consumption styles: SCRIPTED AUTOMATION (Expect, ReadLine, WriteLine)
and INTERACTIVE TERMINAL HOSTING (Read / Write plus the DataReceived event).

    public sealed class ShellStream : Stream
    {
        public event EventHandler<ShellDataEventArgs> DataReceived;
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;
        public event EventHandler<EventArgs> Closed;

        public bool DataAvailable { get; }
        public bool AutoFlush { get; set; }               // default false
        public bool DisableReadBuffering { get; set; }    // default false

        public void ChangeWindowSize(uint columns, uint rows, uint width, uint height);

        public string Read();                             // all buffered text
        public override int Read(byte[] buffer, int offset, int count);
        public override int Read(Span<byte> buffer);
        public override int ReadByte();
        public string ReadLine();
        public string ReadLine(TimeSpan timeout);

        public string Expect(string text);
        public string Expect(string text, TimeSpan timeout, int lookback = -1);
        public string Expect(Regex regex);
        public string Expect(Regex regex, TimeSpan timeout, int lookback = -1);
        public void Expect(params ExpectAction[] expectActions);
        public void Expect(TimeSpan timeout, params ExpectAction[] expectActions);
        public void Expect(TimeSpan timeout, int lookback, params ExpectAction[] expectActions);
        public IAsyncResult BeginExpect(params ExpectAction[] expectActions);
        public IAsyncResult BeginExpect(AsyncCallback callback, params ExpectAction[] expectActions);
        public IAsyncResult BeginExpect(AsyncCallback callback, object state,
                                        params ExpectAction[] expectActions);
        public IAsyncResult BeginExpect(TimeSpan timeout, AsyncCallback callback, object state,
                                        params ExpectAction[] expectActions);
        public IAsyncResult BeginExpect(TimeSpan timeout, int lookback, AsyncCallback callback,
                                        object state, params ExpectAction[] expectActions);
        public string EndExpect(IAsyncResult asyncResult);

        public void Write(string text);                   // always flushes
        public void WriteLine(string line);               // always flushes
        public override void Write(byte[] buffer, int offset, int count);
        public override void Write(ReadOnlySpan<byte> buffer);
        public override void WriteByte(byte value);
        public override Task WriteAsync(byte[] buffer, int offset, int count,
                                        CancellationToken cancellationToken);
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
                                             CancellationToken cancellationToken = default);
        public void WriteAndFlush(string text);
        public void WriteAndFlush(byte[] buffer, int offset, int count);
        public override void Flush();
    }

    public class ExpectAction
    {
        public ExpectAction(Regex expect, Action<string> action);
        public ExpectAction(string expect, Action<string> action);
        public Regex Expect { get; }
        public Action<string> Action { get; }
    }

    using ShellStream shell = client.CreateShellStream(
        "xterm-256color", columns: 120, rows: 30, width: 0, height: 0,
        bufferSize: 4096);

The terminalName argument is passed through verbatim and becomes TERM on the
remote side. The seven-parameter overload adds
IDictionary<TerminalModes, uint> terminalModeValues, which is the SSH
pseudo-terminal mode map: TerminalModes is a byte enum with the RFC 4254
opcodes (ECHO, ICANON, ISIG, ICRNL, ONLCR, OPOST, IXON, IUTF8, VINTR, VEOF,
CS8, PARENB, TTY_OP_ISPEED, TTY_OP_OSPEED, ... plus TTY_OP_END = 0). Passing
`new Dictionary<TerminalModes, uint> { { TerminalModes.ECHO, 0 } }` requests a
terminal with echo off, which is what you want when the host application is
already echoing keystrokes.

SCRIPTED AUTOMATION. Expect blocks until the pattern appears in the buffered
text and returns everything up to and including the match; the ExpectAction
overloads dispatch to the first matching pattern:

    shell.Expect("$ ");
    shell.WriteLine("sudo -k systemctl restart nginx");
    shell.Expect(new ExpectAction("[Pp]assword", _ => shell.WriteLine(password)),
                 new ExpectAction("$ ", _ => { }));

  * The lookback parameter limits how far back in the buffered text the match
    may start; -1 means "the whole buffer".
  * Without a timeout, Expect and ReadLine block indefinitely. Prefer the
    TimeSpan overloads in unattended code.

INTERACTIVE CONSUMPTION PATTERN (terminal hosts). Use a dedicated reader
thread in a blocking Read() loop. Read() blocks until output arrives and
returns 0 when the channel closes, which doubles as the disconnect signal:

    var buffer = new byte[4096];
    int n;
    while ((n = shell.Read(buffer, 0, buffer.Length)) > 0)
    {
        terminal.Feed(buffer, 0, n);    // render the chunk
    }
    // n == 0: channel closed -- tear down the session UI here.

The ErrorOccurred event supplies a human-readable reason to show when the read
loop ends abnormally, and Closed fires when the channel closes.

PITFALL -- DataReceived does not replace Read(). Every incoming chunk is
committed to the internal read buffer AND raised via DataReceived
(ShellDataEventArgs.Data is the byte[], Line the decoded string); the two are
not alternatives. A consumer that subscribes to DataReceived and never calls
Read() leaves every byte of session output accumulating in the internal read
buffer for the life of the session, growing it without bound. Event-style
consumers must opt in to DisableReadBuffering, or simply use the Read() loop
above.

DisableReadBuffering (opt-in, default false; no upstream counterpart). When
set to true, incoming data is delivered solely via DataReceived and is never
committed to the internal read buffer, eliminating the unbounded growth
described above. While it is set, the buffer-reading members (Read, ReadLine,
Expect, BeginExpect and friends) throw InvalidOperationException rather than
block on a buffer that will never fill -- it is strictly for event-style
consumers, and a thread already blocked in one of those members is woken and
throws. Set it once, immediately after CreateShellStream. Setting it to true
discards anything already buffered; setting it back to false resumes buffering
from that point.

WRITES ARE BUFFERED -- call Flush to send. The byte-oriented writes
(Write(byte[], int, int), Write(ReadOnlySpan<byte>), WriteByte, WriteAsync)
accumulate in a write buffer and reach the wire only when Flush() is called or
the buffer fills. Nothing fails when Flush is forgotten; the session just sits
silent. The string-oriented Write(string) and WriteLine(string) always flush
automatically. Two conveniences (no upstream counterpart) remove the
boilerplate for terminal hosts, where every keystroke batch must be flushed:

    shell.AutoFlush = true;           // byte-oriented writes now flush
    shell.WriteAndFlush(bytes, 0, n); // explicit write-plus-flush
    shell.WriteAndFlush(text);        //   (byte[] and string overloads)

ChangeWindowSize(columns, rows, width, height) sends the PTY window-change
request for live terminal resizing. It is easy to assume missing (the upstream
project's issue tracker carries long-open "no public resize" discussions), but
it is present and works; call it whenever the hosting control is resized.
After Read/Write it is the most important ShellStream member for interactive
use. Column/row dimensions override the pixel dimensions when non-zero.

WriteLine sends "\r" after the text on a pseudo-terminal stream and "\n" on a
CreateShellStreamNoTerminal stream, matching what each remote side expects.

Shell is the older, stream-wiring alternative: you supply the input, output
and extendedOutput streams up front and the class pumps them.

    public sealed class Shell : IDisposable
    {
        public bool IsStarted { get; }
        public event EventHandler<EventArgs> Starting;
        public event EventHandler<EventArgs> Started;
        public event EventHandler<EventArgs> Stopping;
        public event EventHandler<EventArgs> Stopped;
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;
        public void Start();      // throws SshException if already started
        public void Stop();       // throws SshException if not started
        public void Dispose();
    }

Prefer ShellStream unless you specifically want the pump-into-my-streams
shape.


PORT FORWARDING
--------------------------------------------------------------------------------

Three concrete forwarded-port types share the abstract base:

    public abstract class ForwardedPort : IForwardedPort
    {
        public abstract bool IsStarted { get; }
        public event EventHandler Closing;
        public event EventHandler<ExceptionEventArgs> Exception;
        public event EventHandler<PortForwardEventArgs> RequestReceived;
        public virtual void Start();
        public virtual void Stop();
        public void Dispose();
    }

    public sealed class PortForwardEventArgs : EventArgs
    {
        public string OriginatorHost { get; }
        public uint OriginatorPort { get; }
    }

    // -L: listen locally, tunnel to host:port as seen from the SSH server
    public partial class ForwardedPortLocal : ForwardedPort
    {
        public ForwardedPortLocal(uint boundPort, string host, uint port);
        public ForwardedPortLocal(string boundHost, string host, uint port);
        public ForwardedPortLocal(string boundHost, uint boundPort, string host, uint port);
        public string BoundHost { get; }
        public uint BoundPort { get; }
        public string Host { get; }
        public uint Port { get; }
    }

    // -R: the SERVER listens and tunnels back to host:port as seen from here
    public class ForwardedPortRemote : ForwardedPort
    {
        public ForwardedPortRemote(uint boundPort, string host, uint port);
        public ForwardedPortRemote(string boundHost, uint boundPort, string host, uint port);
        public ForwardedPortRemote(IPAddress boundHostAddress, uint boundPort,
                                   IPAddress hostAddress, uint port);
        public string BoundHost { get; }
        public IPAddress BoundHostAddress { get; }
        public uint BoundPort { get; }
        public string Host { get; }
        public IPAddress HostAddress { get; }
        public uint Port { get; }
    }

    // -D: a local SOCKS4/SOCKS5 proxy that tunnels through the SSH connection
    public class ForwardedPortDynamic : ForwardedPort
    {
        public ForwardedPortDynamic(uint port);
        public ForwardedPortDynamic(string host, uint port);
        public string BoundHost { get; }
        public uint BoundPort { get; }
    }

The lifecycle is always the same:

    using var client = new SshClient("gateway", "user", key);
    client.Connect();                                   // 1. connect FIRST

    var tunnel = new ForwardedPortLocal("127.0.0.1", 15432, "db.internal", 5432);
    tunnel.Exception += (sender, e) => log.Error(e.Exception, "tunnel error");
    tunnel.RequestReceived += (sender, e) =>
        log.Info("connection from {Host}:{Port}", e.OriginatorHost, e.OriginatorPort);

    client.AddForwardedPort(tunnel);                    // 2. attach
    tunnel.Start();                                     // 3. start
    // ... use localhost:15432 ...
    tunnel.Stop();
    client.RemoveForwardedPort(tunnel);

  * AddForwardedPort throws if the client is NOT CONNECTED, and Start() throws
    InvalidOperationException if the port was never added to a client
    ("Forwarded port is not added to a client"), or SshConnectionException if
    the client has since disconnected.
  * ForwardedPortLocal's (uint boundPort, string host, uint port) overload
    leaves the bound host empty, which is resolved with
    Dns.GetHostAddresses(""); pass "127.0.0.1" explicitly to be sure a tunnel
    stays private to the machine. ForwardedPortDynamic with no host binds
    IPAddress.Any -- every local interface.
  * Passing boundPort 0 to ForwardedPortLocal asks the OS for a free port;
    read BoundPort after Start() to learn which one.
  * A forwarded port belongs to one client -- attaching an already-attached
    port to a different client throws InvalidOperationException.
  * Errors on the tunnel's own background threads surface only through the
    Exception event; there is no call to catch them from. Always subscribe.
  * Disconnecting or disposing the client stops its forwarded ports.


NETCONF
--------------------------------------------------------------------------------

    public class NetConfClient : BaseClient
    {
        public TimeSpan OperationTimeout { get; set; }         // default infinite
        public bool AutomaticMessageIdHandling { get; set; }   // default true
        public XmlDocument ServerCapabilities { get; }
        public XmlDocument ClientCapabilities { get; }
        public XmlDocument SendReceiveRpc(XmlDocument rpc);
        public XmlDocument SendReceiveRpc(string xml);
        public XmlDocument SendCloseRpc();
    }

Same five constructor shapes as the other clients. The NETCONF subsystem is
negotiated on Connect(); ServerCapabilities is populated from the server's
hello. With AutomaticMessageIdHandling left true the client assigns and
matches message-id attributes for you. Server-side faults arrive as
NetConfServerException.


LOGGING
--------------------------------------------------------------------------------

    public static class SshNetLoggingConfiguration
    {
        public static void InitializeLogging(ILoggerFactory loggerFactory);
    }

    SshNetLoggingConfiguration.InitializeLogging(loggerFactory);   // process-wide

Per-connection logging can be set instead via ConnectionInfo.LoggerFactory,
which takes precedence for that connection. Without either, logging is a no-op
(a NullLoggerFactory). Logger categories are the full type names, so they are
rooted at "CodeBrix.SSH.*" -- filter on that prefix.


ERROR MODEL
--------------------------------------------------------------------------------

All library exceptions derive from CodeBrix.SSH.Common.SshException:

    SshException
      +- SshConnectionException          // .DisconnectReason (enum, Messages.Transport)
      +- SshAuthenticationException
      +- SshOperationTimeoutException
      +- SshPassPhraseNullOrEmptyException
      +- SftpException                   // .StatusCode (CodeBrix.SSH.Sftp.StatusCode)
      |    +- SftpPathNotFoundException
      |    +- SftpPermissionDeniedException
      +- ScpException
      +- ProxyException
      +- NetConfServerException

Argument validation throws the usual BCL types (ArgumentNullException,
ArgumentException, ArgumentOutOfRangeException), disposed clients throw
ObjectDisposedException, and misuse of the streaming APIs throws
InvalidOperationException. Errors raised on the library's background threads
cannot be caught at a call site -- they arrive on BaseClient.ErrorOccurred,
ShellStream.ErrorOccurred, Shell.ErrorOccurred or ForwardedPort.Exception,
each carrying an ExceptionEventArgs with a single Exception property.


INTERFACES FOR TESTING AND MOCKING
--------------------------------------------------------------------------------

Every client is constructed against a concrete socket, so unit tests should
depend on the interfaces rather than the classes:

    public interface IBaseClient : IDisposable
    {
        ConnectionInfo ConnectionInfo { get; }
        bool IsConnected { get; }
        TimeSpan KeepAliveInterval { get; set; }
        event EventHandler<ExceptionEventArgs> ErrorOccurred;
        event EventHandler<HostKeyEventArgs> HostKeyReceived;
        event EventHandler<SshIdentificationEventArgs> ServerIdentificationReceived;
        void Connect();
        Task ConnectAsync(CancellationToken cancellationToken);
        void Disconnect();
        void SendKeepAlive();
    }

    public interface ISshClient : IBaseClient   // implemented by SshClient
    public interface ISftpClient : IBaseClient  // implemented by SftpClient

ISshClient declares ForwardedPorts, AddForwardedPort, RemoveForwardedPort,
CreateCommand, RunCommand, and every CreateShell / CreateShellStream overload.
ISftpClient declares BufferSize, OperationTimeout, ProtocolVersion,
WorkingDirectory and the whole SFTP method surface listed above. Take
ISshClient / ISftpClient in your own types, mock those in tests, and construct
the concrete client only at the composition root.

ScpClient and NetConfClient have NO interface -- they can only be faked by
subclassing, and their members are not virtual, so wrap them in an interface
of your own if you need to test around them.

RemotePathTransformation (IRemotePathTransformation) and ForwardedPort are
also useful seams: the former is a one-method interface, the latter an
abstract class you can substitute.


PUBLIC PLUMBING YOU WILL NOT NEED
--------------------------------------------------------------------------------

Roughly a third of the assembly's public types are protocol internals that are
public only because the library is layered, not because a consumer is meant to
use them. They are listed here so you can recognise them and move on:

  * SSH wire messages (CodeBrix.SSH.Messages and its .Authentication,
    .Connection and .Transport children): the abstract Message,
    ChannelMessage, RequestMessage, RequestInfo and ChannelOpenInfo bases and
    their ~35 concrete messages -- KeyExchangeInitMessage, NewKeysMessage,
    DisconnectMessage, DebugMessage, IgnoreMessage, UnimplementedMessage,
    ServiceRequestMessage / ServiceAcceptMessage, SuccessMessage /
    FailureMessage, BannerMessage, RequestMessagePublicKey,
    GlobalRequestMessage, RequestSuccessMessage / RequestFailureMessage, the
    Channel* messages (Open, OpenConfirmation, OpenFailure, Data,
    ExtendedData, Eof, Close, Request, Success, Failure, WindowAdjust), the
    KeyExchange*ReplyMessage family, and the RequestInfo subclasses
    (KeepAliveRequestInfo, EndOfWriteRequestInfo and their siblings). The
    enums ServiceName, GlobalRequestName and the marker interface
    IKeyExchangedAllowed belong to the same layer; DisconnectReason is the
    only member of it consumers routinely read, via
    SshConnectionException.DisconnectReason. MessageEventArgs<T> wraps a
    message for the internal event plumbing.

  * Algorithm plumbing (CodeBrix.SSH.Security and below): the abstract
    Algorithm, KeyExchange, Cipher, SymmetricCipher, BlockCipher and
    CipherMode bases, the concrete KeyExchangeDiffieHellman /
    KeyExchangeDiffieHellmanGroupExchange, the cipher modes CbcCipherMode,
    CfbCipherMode, CtrCipherMode and OfbCipherMode, and AesCipherMode. You
    reach these only through the ConnectionInfo algorithm dictionaries, and
    only by name.

  * APM result types (CodeBrix.SSH.Common.AsyncResult, AsyncResult<T>, and the
    SFTP-specific SftpDownloadAsyncResult, SftpUploadAsyncResult,
    SftpListDirectoryAsyncResult, SftpSynchronizeDirectoriesAsyncResult). The
    Begin*/End* methods declare IAsyncResult, so you never name these; pass
    the value straight back to the matching End* method.

  * Session, SshData, SshDataStream and PipeStream are internal machinery
    exposed for the same layering reasons. SshCommand.OutputStream and
    ExtendedOutputStream are PipeStream instances, but you should treat them
    simply as Stream.

Prefer the Task-based APIs over the APM ones in new code; the APM surface
exists for compatibility with the upstream library's history.


COMPLETE EXAMPLES
================================================================================

EXAMPLE 1 -- Connect with a password or a private key and run a command
--------------------------------------------------------------------------------

    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CodeBrix.SSH;
    using CodeBrix.SSH.Common;

    namespace Demo;

    public static class RunCommandDemo
    {
        public static async Task<int> RunAsync(string host, string user,
                                               string passwordOrKeyPath,
                                               bool useKey,
                                               CancellationToken cancellationToken)
        {
            using SshClient client = useKey
                ? new SshClient(host, user, new PrivateKeyFile(passwordOrKeyPath))
                : new SshClient(host, user, passwordOrKeyPath);

            client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(15);
            client.KeepAliveInterval = TimeSpan.FromSeconds(30);
            client.HostKeyReceived += (sender, e) =>
            {
                Console.WriteLine($"host key {e.HostKeyName} SHA256:{e.FingerPrintSHA256}");
                e.CanTrust = true;    // replace with a real policy -- see Example 6
            };

            try
            {
                await client.ConnectAsync(cancellationToken);

                using SshCommand cmd = client.CreateCommand("uname -a && id");
                cmd.CommandTimeout = TimeSpan.FromSeconds(30);
                await cmd.ExecuteAsync(cancellationToken);

                Console.WriteLine(cmd.Result);

                if (cmd.ExitStatus != 0)
                {
                    Console.Error.WriteLine(cmd.Error);
                }

                return cmd.ExitStatus ?? -1;
            }
            catch (SshAuthenticationException ex)
            {
                Console.Error.WriteLine($"authentication failed: {ex.Message}");
                return -1;
            }
            catch (SshConnectionException ex)
            {
                Console.Error.WriteLine($"connection failed ({ex.DisconnectReason}): {ex.Message}");
                return -1;
            }
            finally
            {
                client.Disconnect();
            }
        }
    }


EXAMPLE 2 -- Multi-factor: password plus keyboard-interactive one-time code
--------------------------------------------------------------------------------

    using System;
    using CodeBrix.SSH;
    using CodeBrix.SSH.Common;

    namespace Demo;

    public static class MultiFactorDemo
    {
        public static SshClient CreateClient(string host, string user, string password,
                                             Func<string> readOneTimeCode)
        {
            var passwordMethod = new PasswordAuthenticationMethod(user, password);

            var keyboardMethod = new KeyboardInteractiveAuthenticationMethod(user);
            keyboardMethod.AuthenticationPrompt += (sender, e) =>
            {
                foreach (AuthenticationPrompt prompt in e.Prompts)
                {
                    // Every prompt MUST get a Response, or authentication throws.
                    prompt.Response = prompt.Request.IndexOf("password",
                                          StringComparison.OrdinalIgnoreCase) >= 0
                        ? password
                        : readOneTimeCode();
                }
            };

            var info = new ConnectionInfo(host, 22, user, passwordMethod, keyboardMethod);
            info.AuthenticationBanner += (sender, e) => Console.WriteLine(e.BannerMessage);

            // The client owns neither method nor info here, so dispose them with it.
            return new SshClient(info);
        }
    }


EXAMPLE 3 -- Interactive shell over ShellStream (terminal host)
--------------------------------------------------------------------------------

    using System;
    using System.Text;
    using System.Threading;
    using CodeBrix.SSH;
    using CodeBrix.SSH.Common;

    namespace Demo;

    public sealed class TerminalSession : IDisposable
    {
        private readonly SshClient _client;
        private readonly ShellStream _shell;
        private readonly Thread _reader;
        private volatile bool _closed;

        public TerminalSession(string host, string user, string password,
                               uint columns, uint rows, Action<byte[], int> onOutput)
        {
            _client = new SshClient(host, user, password);
            _client.Connect();

            var modes = new System.Collections.Generic.Dictionary<TerminalModes, uint>
            {
                { TerminalModes.ECHO, 0 },      // the host application echoes
            };

            _shell = _client.CreateShellStream("xterm-256color", columns, rows,
                                               width: 0, height: 0,
                                               bufferSize: 4096,
                                               terminalModeValues: modes);
            _shell.AutoFlush = true;
            _shell.ErrorOccurred += (sender, e) => Console.Error.WriteLine(e.Exception.Message);
            _shell.Closed += (sender, e) => _closed = true;

            _reader = new Thread(() =>
            {
                var buffer = new byte[4096];
                int n;
                while ((n = _shell.Read(buffer, 0, buffer.Length)) > 0)
                {
                    onOutput(buffer, n);
                }
                _closed = true;             // n == 0: channel closed
            })
            {
                IsBackground = true,
                Name = "ssh-shell-reader",
            };
            _reader.Start();
        }

        public bool IsClosed => _closed;

        public void SendKeys(string text)
        {
            _shell.WriteAndFlush(text);
        }

        public void SendBytes(byte[] buffer, int count)
        {
            _shell.WriteAndFlush(buffer, 0, count);
        }

        public void Resize(uint columns, uint rows)
        {
            _shell.ChangeWindowSize(columns, rows, width: 0, height: 0);
        }

        public void Dispose()
        {
            _shell.Dispose();
            _client.Dispose();
        }
    }


EXAMPLE 4 -- SFTP upload and download with progress
--------------------------------------------------------------------------------

    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using CodeBrix.SSH;
    using CodeBrix.SSH.Sftp;

    namespace Demo;

    public static class SftpTransferDemo
    {
        public static async Task TransferAsync(string host, string user, string keyPath,
                                               string localFile, string remoteDir,
                                               CancellationToken cancellationToken)
        {
            using var client = new SftpClient(host, user, new PrivateKeyFile(keyPath));
            client.BufferSize = 64 * 1024;                     // fewer round trips
            client.OperationTimeout = TimeSpan.FromMinutes(5);
            client.HostKeyReceived += (sender, e) => e.CanTrust = true;

            await client.ConnectAsync(cancellationToken);

            string remoteFile = $"{remoteDir}/{Path.GetFileName(localFile)}";

            if (!await client.ExistsAsync(remoteDir, cancellationToken))
            {
                await client.CreateDirectoryAsync(remoteDir, cancellationToken);
            }

            long total = new FileInfo(localFile).Length;
            var uploadProgress = new Progress<UploadFileProgressReport>(
                r => Console.Write($"\rup   {r.TotalBytesUploaded}/{total}"));

            using (FileStream source = File.OpenRead(localFile))
            {
                await client.UploadFileAsync(source, remoteFile, canOverride: true,
                                             uploadProgress, cancellationToken);
            }

            Console.WriteLine();

            var downloadProgress = new Progress<DownloadFileProgressReport>(
                r => Console.Write($"\rdown {r.TotalBytesDownloaded}/{total}"));

            using (FileStream destination = File.Create(localFile + ".roundtrip"))
            {
                await client.DownloadFileAsync(remoteFile, destination,
                                               downloadProgress, cancellationToken);
            }

            Console.WriteLine();

            await foreach (ISftpFile entry in client.ListDirectoryAsync(remoteDir,
                                                                       cancellationToken))
            {
                if (entry.Name is "." or "..")
                {
                    continue;
                }

                Console.WriteLine($"{entry.Attributes}  {entry.FullName}");
            }

            client.Disconnect();
        }
    }

To refuse to overwrite, note the argument order -- uploadProgress comes before
the token, so name the token:

    await client.UploadFileAsync(source, remoteFile, canOverride: false,
                                 cancellationToken: cancellationToken);


EXAMPLE 5 -- Local port forward (ssh -L) to a database behind a gateway
--------------------------------------------------------------------------------

    using System;
    using CodeBrix.SSH;

    namespace Demo;

    public static class TunnelDemo
    {
        public static void WithTunnel(string gateway, string user, string keyPath,
                                      Action<uint> useLocalPort)
        {
            using var client = new SshClient(gateway, user, new PrivateKeyFile(keyPath));
            client.HostKeyReceived += (sender, e) => e.CanTrust = true;
            client.Connect();                    // must be connected BEFORE AddForwardedPort

            // boundPort 0 -> let the OS pick; read BoundPort after Start().
            var tunnel = new ForwardedPortLocal("127.0.0.1", 0, "db.internal", 5432);
            tunnel.Exception += (sender, e) =>
                Console.Error.WriteLine($"tunnel error: {e.Exception.Message}");
            tunnel.RequestReceived += (sender, e) =>
                Console.WriteLine($"client {e.OriginatorHost}:{e.OriginatorPort}");

            client.AddForwardedPort(tunnel);
            tunnel.Start();

            try
            {
                // Connect your database driver to 127.0.0.1:tunnel.BoundPort
                useLocalPort(tunnel.BoundPort);
            }
            finally
            {
                tunnel.Stop();
                client.RemoveForwardedPort(tunnel);
                tunnel.Dispose();
            }
        }
    }

A dynamic (SOCKS) forward is the same shape:

    var socks = new ForwardedPortDynamic("127.0.0.1", 1080);
    client.AddForwardedPort(socks);
    socks.Start();      // point a SOCKS5-aware client at 127.0.0.1:1080


EXAMPLE 6 -- Strict known_hosts verification
--------------------------------------------------------------------------------

    using System;
    using CodeBrix.SSH;
    using CodeBrix.SSH.Common;
    using CodeBrix.SSH.KnownHosts;

    namespace Demo;

    public static class KnownHostsDemo
    {
        public static void ConnectStrict(string host, string user, string keyPath)
        {
            // Use the user's real OpenSSH file, or pass an app-specific path.
            var store = new KnownHostsStore(KnownHostsStore.DefaultFilePath);

            using var client = new SshClient(host, user, new PrivateKeyFile(keyPath));

            // Exactly one policy call, after construction and before Connect().
            client.UseStrictHostKeyVerification(store);

            try
            {
                client.Connect();
            }
            catch (SshConnectionException)
            {
                // Rejected host key: the key is unknown, changed or revoked.
                // Inspect it first, then decide whether to enrol it.
                throw;
            }
        }

        public static void ConnectTrustOnFirstUse(string host, string user, string password)
        {
            var store = new KnownHostsStore(KnownHostsStore.DefaultFilePath);

            using var client = new SshClient(host, user, password);

            client.UseTrustOnFirstUse(store, mismatch =>
            {
                Console.Error.WriteLine(
                    $"HOST KEY {mismatch.Result} for {mismatch.Host}:{mismatch.Port} " +
                    $"({mismatch.KeyAlgorithm}) SHA256:{mismatch.FingerPrintSHA256}");

                return false;      // true would connect anyway (store untouched)
            });

            client.Connect();
        }
    }


EXAMPLE 7 -- Piping data into a remote command
--------------------------------------------------------------------------------

    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using CodeBrix.SSH;

    namespace Demo;

    public static class StdinDemo
    {
        public static async Task<string> GzipRemotelyAsync(ISshClient client, byte[] payload,
                                                           CancellationToken cancellationToken)
        {
            using SshCommand cmd = client.CreateCommand("gzip -c | base64 -w0");

            Task running = cmd.ExecuteAsync(cancellationToken);

            using (Stream stdin = cmd.CreateInputStream())
            {
                await stdin.WriteAsync(payload, cancellationToken);
            }   // disposing stdin signals EOF -- without this the command never ends

            await running;

            return cmd.Result;
        }
    }


MINIMUM VIABLE PROJECT
================================================================================

Demo.csproj:

    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="CodeBrix.SSH.MitLicenseForever" Version="*" />
      </ItemGroup>
    </Project>

Program.cs:

    using System;
    using CodeBrix.SSH;
    using CodeBrix.SSH.KnownHosts;

    namespace Demo;

    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("usage: demo <host> <user> <password> [command]");
                return 2;
            }

            var store = new KnownHostsStore(KnownHostsStore.DefaultFilePath);

            using var client = new SshClient(args[0], args[1], args[2]);
            client.UseTrustOnFirstUse(store);
            client.Connect();

            using SshCommand cmd = client.RunCommand(args.Length > 3 ? args[3] : "uname -a");
            Console.Write(cmd.Result);
            Console.Error.Write(cmd.Error);

            return cmd.ExitStatus ?? 0;
        }
    }

The "*" wildcard resolves to the newest published package; running
`dotnet add package CodeBrix.SSH.MitLicenseForever` writes a concrete pin
instead.


PERFORMANCE TIPS
================================================================================

  * REUSE ONE CONNECTION. Connect() performs a TCP handshake, a version
    exchange, a key exchange and authentication -- tens to hundreds of
    milliseconds. Commands, SFTP operations and shells all multiplex over
    channels on one authenticated session, so hold a client open and open
    channels, rather than reconnecting per operation. ConnectionInfo.MaxSessions
    (default 10) is a semaphore over concurrent SESSION channels -- commands,
    shells and the SFTP / NETCONF subsystems -- so opening an eleventh BLOCKS
    until one is released. Raise it if you fan out. Forwarded-port channels
    are not counted against it.

  * SftpClient.BufferSize (default 32 KB) is the single biggest SFTP transfer
    knob. Each SFTP read or write is a request/response round trip, so
    throughput on a high-latency link is roughly the per-request payload
    divided by the round-trip time. There is a CEILING, though: the effective
    read length is min(BufferSize, the channel's local data packet size) minus
    13 protocol bytes, and the effective write length is min(BufferSize, the
    packet size the SERVER advertised for the channel) minus 25 bytes and the
    file handle length. The local packet size is 64 KB, so raising BufferSize
    beyond that buys nothing for reads, and writes are additionally capped by
    whatever the server advertised. 64 KB is the practical maximum worth
    setting. ScpClient.BufferSize (default 16 KB) is the equivalent knob for
    SCP.

  * OperationTimeout defaults to Timeout.InfiniteTimeSpan on SftpClient,
    ScpClient and NetConfClient, and SshCommand.CommandTimeout to the same. An
    unattended process should set finite values, or a wedged server hangs it
    forever. ConnectionInfo.Timeout (default 30 seconds) covers only the
    connect and per-message waits.

  * KeepAliveInterval keeps NAT and firewall state alive on idle connections.
    30 seconds is a sane value for long-lived interactive sessions; it costs
    one tiny packet per interval. It is disabled by default.

  * PREFER THE ASYNC METHODS on hot paths. The synchronous SFTP calls block a
    thread pool thread for the whole round trip; the async ones do not. The
    APM (Begin*/End*) methods exist for compatibility and offer no advantage
    over the Task-based ones.

  * STREAM LARGE FILES rather than materialising them. ReadAllBytes /
    ReadAllText pull the whole file into memory; OpenRead / Open return an
    SftpFileStream that composes with CopyToAsync and StreamReader.

  * For directory trees, prefer many parallel SFTP operations on ONE client
    over one operation on many clients -- the channel is cheap, the session is
    not.

  * ALGORITHM CHOICE MATTERS on constrained hardware: chacha20-poly1305 and
    the AES-GCM ciphers are AEAD (one pass, no separate MAC), and AES-CTR/GCM
    use hardware AES where the CPU has it. Reordering ConnectionInfo.Encryptions
    changes what is negotiated. Removing 3des-cbc and the SHA-1 MACs is a
    security win with no performance cost.

  * SHELLSTREAM BUFFER SIZE is the INITIAL capacity of the read and write
    buffers, which grow on demand; 4096 is a good starting point for a
    terminal. Sizing it well matters far less than draining it -- see the
    DisableReadBuffering pitfall below.

  * Logging is a no-op until you call InitializeLogging (or set
    ConnectionInfo.LoggerFactory). Do not wire a verbose logger in production
    hot paths; the library logs every message sent and received at trace
    level.


COMMON PITFALLS TO AVOID
================================================================================

 1. HOST KEYS ARE TRUSTED BY DEFAULT. HostKeyEventArgs.CanTrust starts true,
    so a client with no HostKeyReceived subscriber accepts any host key --
    including an attacker's. Wire a policy (KnownHosts extension, pinned
    fingerprint, or an explicit accept-all you have consciously chosen).

 2. Do not add a BouncyCastle package reference. The cryptography dependency
    is CodeBrix.Cryptography.MitLicenseForever and its namespaces are rooted
    at CodeBrix.Cryptography, not Org.BouncyCastle. Adding BouncyCastle back
    gives you two copies of the same types and ambiguous references.

 3. Do not write upstream namespaces. `using Renci.SshNet;` does not compile
    against this package -- the type names are identical but the namespace is
    CodeBrix.SSH.

 4. ShellStream writes are BUFFERED. Byte-oriented Write / WriteByte /
    WriteAsync do not reach the wire until Flush() or a full buffer. Nothing
    throws; the session simply appears dead. Set AutoFlush = true or use
    WriteAndFlush. (Write(string) and WriteLine(string) always flush.)

 5. Subscribing to ShellStream.DataReceived does NOT stop the internal read
    buffer from filling. Both happen. Either drain with Read(), or set
    DisableReadBuffering = true -- and then never call Read/ReadLine/Expect,
    which throw InvalidOperationException while it is set.

 6. AddForwardedPort throws unless the client is already CONNECTED, and
    ForwardedPort.Start() throws unless the port has been added to a connected
    client. The order is Connect -> AddForwardedPort -> Start.

 7. Errors on background threads never reach your call site. Subscribe to
    BaseClient.ErrorOccurred, ForwardedPort.Exception, ShellStream.ErrorOccurred
    and Shell.ErrorOccurred, or you will debug silent tunnels.

 8. SftpClient.UploadFileAsync's canOverride overload takes uploadProgress
    BEFORE the CancellationToken. Positional code that omits the progress
    binds the token to the wrong parameter or fails to compile -- pass
    `cancellationToken: token` by name.

 9. SshCommand.Result and .Error each drain their stream once and cache the
    result. Do not read Result and then expect OutputStream to still hold
    data, and do not read OutputStream and then expect Result to be populated.

10. A stdin stream from CreateInputStream must be DISPOSED to signal EOF.
    Without it the remote command may wait for input forever, and your
    ExecuteAsync never completes. CreateInputStream is also valid only while
    the command is executing, and only once.

11. SshCommand, ShellStream, Shell, SftpFileStream and every client are
    IDisposable and hold a channel or a socket. Leaking them exhausts the
    MaxSessions semaphore, and the next command or shell then BLOCKS
    indefinitely waiting for a slot -- a hang that looks like a server
    problem but is a missing Dispose.

12. SynchronizeDirectories is not a mirror. It is one-way (local to remote),
    non-recursive, size-comparison only, and never deletes anything on the
    remote side.

13. Expect and ReadLine without a TimeSpan block forever if the expected text
    never arrives. Always pass a timeout in unattended code, and remember that
    Expect matches against the buffered text, so a prompt already consumed by
    a previous Read() will not match.

14. ConnectionInfo algorithm dictionaries are filled by the constructor.
    Mutating them before construction is impossible and after Connect() is
    pointless -- do it in between.

15. CreateCommand(text, encoding) assigns ConnectionInfo.Encoding as a side
    effect, changing the encoding for later commands on that client. Set
    ConnectionInfo.Encoding once if you want one encoding throughout.

16. The library ships no nullable-reference annotations. To a consumer that
    enables NRT, its reference types are "oblivious": you get no warnings, and
    also no protection. Null-check what the documentation says can be null
    (SshCommand.ExitStatus is an int? and IS null until the server reports
    one; ExitSignal, HostKeyEventArgs.Certificate and PrivateKeyFile.Certificate
    are null when absent).

17. ScpClient is synchronous only and has no interface. If you need
    cancellation, progress objects or mockability, use SftpClient.

18. Remote paths passed to ScpClient are interpolated into a server-side shell
    command. Leave RemotePathTransformation at DoubleQuote (or move to
    ShellQuote); setting it to None with untrusted path input is a command
    injection.

19. IsConnected on SftpClient is stricter than on the other clients: it also
    requires the SFTP subsystem session to be open, so a transport-level
    connection is not enough.

20. Disposing a ConnectionInfo you passed to a client is your job -- the
    convenience constructors own the ConnectionInfo they build internally, but
    a ConnectionInfo you construct is not owned by the client. The
    IDisposable ConnectionInfo subclasses (PasswordConnectionInfo,
    PrivateKeyConnectionInfo, KeyboardInteractiveConnectionInfo) and the
    AuthenticationMethod instances you build should be disposed with the
    client.


WHAT THIS PACKAGE DOES NOT DO
================================================================================

  * It is an SSH CLIENT only. There is no server, no sshd, no host-side
    session handling, and no way to accept inbound SSH connections.

  * No host CERTIFICATE AUTHORITY verification. Client-side certificate
    authentication is supported (PrivateKeyFile with a certificate file, and
    the *-cert-v01@openssh.com algorithms), and HostKeyEventArgs.Certificate
    exposes a host certificate when the server sends one, but nothing
    validates it against a CA: KnownHostsStore ignores @cert-authority lines
    and treats a certificate blob as an ordinary key. Verifying a host
    certificate chain is your code's job.

  * No ssh_config / ~/.ssh/config parsing. Hosts, ports, users, identity
    files, ProxyJump and every other OpenSSH client option must be supplied
    programmatically. Only the known_hosts FORMAT is understood, through
    KnownHostsStore.

  * No SSH agent support. There is no ssh-agent / Pageant client, so keys must
    be read from files, streams or Key instances; agent forwarding is not
    implemented.

  * No X11 forwarding for consumers. An X11 channel type exists internally,
    but no public API requests or handles X11 forwarding.

  * No GSSAPI / Kerberos authentication, and no host-based authentication.

  * No ProxyJump / multi-hop chaining as a feature. Reaching a second host
    means either a SOCKS/HTTP proxy through ProxyTypes, or a
    ForwardedPortLocal on the first connection that a second client connects
    through.

  * No rsync, no resumable transfers, no built-in retry, and no bandwidth
    limiting. SynchronizeDirectories is the only synchronization helper and is
    deliberately minimal (see pitfall 12).

  * No connection pooling, keep-alive-on-failure reconnection, or automatic
    session recovery. If a session drops, you construct and connect a new
    client.

  * No SFTP protocol extensions beyond what the SFTP session negotiates, and
    no SFTP v4+ ACL surface.

  * No support for .NET Framework, .NET Standard, or .NET versions below 10.


WORKING EXAMPLES ON GITHUB
================================================================================

The unit test suite is the executable specification for everything above. It
runs offline against in-process fakes, so the tests read as usage examples
with the server stubbed out.

  https://github.com/ellisnet/CodeBrix.SSH/tree/main/tests/CodeBrix.SSH.Tests

Feature-to-file map. Append each entry below to

  https://github.com/ellisnet/CodeBrix.SSH/tree/main/tests/CodeBrix.SSH.Tests/Classes/

to open it:

    SshClient lifecycle, shells, forwarded-port teardown
        SshClientTest.cs
        SshClientTest_CreateShellStream_TerminalNameAndColumnsAndRowsAndWidthAndHeightAndBufferSize_Connected.cs
        SshClientTest_CreateShellStream_TerminalNameAndColumnsAndRowsAndWidthAndHeightAndBufferSizeAndTerminalModes_Connected.cs
        SshClientTest_Disconnect_ForwardedPortStarted.cs
        SshClientTest_Dispose_Connected.cs

    Connect / disconnect / keep-alive on the shared base client
        BaseClientTest_ConnectAsync_Timeout.cs
        BaseClientTest_Connected_KeepAliveInterval_NotNegativeOne.cs
        BaseClientTest_Disconnected_Connect.cs

    SshCommand execution, APM pattern, disposal
        SshCommandTest.cs
        SshCommandTest_BeginExecute_EndExecuteNotInvokedOnAsyncResultFromPreviousInvocation.cs
        SshCommandTest_EndExecute_AsyncResultFromOtherInstance.cs
        SshCommandTest_Dispose.cs

    Authentication methods and multi-factor negotiation
        PasswordAuthenticationMethodTest.cs
        PrivateKeyAuthenticationMethodTest.cs
        KeyboardInteractiveAuthenticationMethodTest.cs
        ClientAuthenticationTest.cs  (plus the ClientAuthenticationTest_Success_*
                                      / _Failure_* files for partial-success paths)
        ConnectionInfoTest.cs, PasswordConnectionInfoTest.cs

    Private key formats and certificates
        PrivateKeyFileTest.cs
        (key fixtures live in tests/CodeBrix.SSH.Tests/Data/)

    known_hosts store and verification policies
        KnownHostsStoreTest.cs
        KnownHostsClientExtensionsTest.cs

    ShellStream: read/expect, write buffering, AutoFlush, DisableReadBuffering
        ShellStreamTest.cs
        ShellStreamTest_ReadExpect.cs
        ShellStreamTest_AutoFlushAndWriteAndFlush.cs
        ShellStreamTest_DisableReadBuffering.cs
        ShellStreamTest_Write_WriteBufferEmptyAndWriteMoreBytesThanBufferSize.cs
          (and the other ShellStreamTest_Write_* buffer-boundary cases)
        ExpectActionTest.cs

    SFTP client operations
        SftpClientTest.cs, SftpClientTest.Connect.cs, SftpClientTest.ConnectAsync.cs
        SftpClientTest.ListDirectory.cs, SftpClientTest.ListDirectoryAsync.cs
        SftpClientTest.UploadFile.cs
        SftpClientTest.GetAttributes.cs, SftpClientTest.GetAttributesAsync.cs
        SftpClientTest.DeleteFile.cs, SftpClientTest.DeleteDirectory.cs
        SftpClientTest_AsyncExceptions.cs
        Sftp/  (the SFTP wire-protocol request and response tests)

    SCP upload and download
        ScpClientTest.cs
        ScpClientTest_Upload_FileInfoAndPath_Success.cs
        ScpClientTest_Download_PathAndStream_SendExecRequestReturnsFalse.cs

    Port forwarding (local, remote, dynamic) across the whole state machine
        ForwardedPortLocalTest.cs, ForwardedPortRemoteTest.cs,
        ForwardedPortDynamicTest.cs and their _Start_* / _Stop_* / _Dispose_*
        siblings

    Host key event data
        Common/HostKeyEventArgsTest.cs

    Security, ciphers and key exchange
        Security/  and  Security/Cryptography/  and
        Security/Cryptography/Ciphers/


QUICK REFERENCE CARD
================================================================================

PACKAGE
    CodeBrix.SSH.MitLicenseForever      MIT      net10.0 only
    dotnet add package CodeBrix.SSH.MitLicenseForever
    Depends on CodeBrix.Cryptography.MitLicenseForever (namespaces rooted at
    CodeBrix.Cryptography, NOT Org.BouncyCastle) and
    Microsoft.Extensions.Logging.Abstractions.

USINGS
    using CodeBrix.SSH;             using CodeBrix.SSH.Sftp;
    using CodeBrix.SSH.Common;      using CodeBrix.SSH.KnownHosts;

CONNECT
    new SshClient(host, user, password)
    new SshClient(host, port, user, password)
    new SshClient(host, user, new PrivateKeyFile(path, passphrase))
    new SshClient(new ConnectionInfo(host, port, user, method1, method2))
    client.Connect() / await client.ConnectAsync(ct) / client.Disconnect()
    client.KeepAliveInterval = TimeSpan.FromSeconds(30)      // default: disabled
    client.ConnectionInfo.Timeout                            // default 30 s

HOST KEYS
    client.HostKeyReceived += (s, e) => e.CanTrust = ...;    // CanTrust starts TRUE
    e.FingerPrintSHA256      e.FingerPrintMD5      e.HostKeyName      e.Certificate
    var store = new KnownHostsStore(KnownHostsStore.DefaultFilePath);
    client.UseStrictHostKeyVerification(store);
    client.UseTrustOnFirstUse(store[, mismatch => bool]);
    HostKeyVerificationResult: Unknown | Known | Mismatch | Revoked

AUTHENTICATION
    new PasswordAuthenticationMethod(user, password)
    new PrivateKeyAuthenticationMethod(user, params IPrivateKeySource[])
    new KeyboardInteractiveAuthenticationMethod(user)   // + AuthenticationPrompt event
    new NoneAuthenticationMethod(user)
    ProxyTypes: None | Socks4 | Socks5 | Http
    new ConnectionInfo(host, port, user, proxyType, proxyHost, proxyPort,
                       proxyUser, proxyPass, params AuthenticationMethod[])

COMMANDS
    using SshCommand cmd = client.RunCommand("uname -a");
    using SshCommand cmd = client.CreateCommand(text[, encoding]);
    cmd.Execute() / await cmd.ExecuteAsync(ct) / cmd.BeginExecute() + EndExecute(ar)
    cmd.Result   cmd.Error   cmd.ExitStatus (int?)   cmd.ExitSignal
    cmd.OutputStream   cmd.ExtendedOutputStream   cmd.CreateInputStream()
    cmd.CommandTimeout (default infinite)   cmd.CancelAsync(forceKill, msTimeout)

SHELL
    using ShellStream sh = client.CreateShellStream("xterm-256color", cols, rows,
                                                    0, 0, 4096[, terminalModes]);
    client.CreateShellStreamNoTerminal(bufferSize = -1)      // like ssh -T
    sh.Read(buffer, 0, len)   // 0 == closed      sh.Read()   sh.ReadLine([timeout])
    sh.Expect(text|regex[, timeout, lookback])   sh.Expect(params ExpectAction[])
    sh.Write(string) / WriteLine(string)         // always flush
    sh.WriteAndFlush(text) / WriteAndFlush(buf, 0, n)        sh.AutoFlush = true
    sh.DisableReadBuffering = true               // event-only consumers
    sh.ChangeWindowSize(cols, rows, 0, 0)
    events: DataReceived, ErrorOccurred, Closed

SFTP
    using var sftp = new SftpClient(host, user, password);
    sftp.BufferSize (default 32 KB)     sftp.OperationTimeout (default infinite)
    UploadFile(stream, path[, canOverride][, Action<ulong>])
    await UploadFileAsync(stream, path[, canOverride][, IProgress<UploadFileProgressReport>], ct)
    DownloadFile(path, stream[, Action<ulong>])
    await DownloadFileAsync(path, stream[, IProgress<DownloadFileProgressReport>], ct)
    ListDirectory(path) / await foreach ListDirectoryAsync(path, ct)  -> ISftpFile
    Get / Exists / GetAttributes / SetAttributes / ChangePermissions / GetStatus
    CreateDirectory / DeleteDirectory / DeleteFile / Delete / RenameFile / SymbolicLink
    Open / OpenRead / OpenWrite / Create -> SftpFileStream ; OpenText / CreateText /
    AppendText -> StreamReader|StreamWriter
    ReadAllText / ReadAllBytes / ReadAllLines / ReadLines / WriteAll* / AppendAll*
    SynchronizeDirectories(local, remote, pattern)   // one-way, flat, size-compare

SCP
    using var scp = new ScpClient(host, user, password);
    scp.Upload(Stream|FileInfo|DirectoryInfo, remotePath)
    scp.Download(remotePath, Stream|FileInfo|DirectoryInfo)
    scp.UseDirectoryFlag = false;              // servers that reject scp "-d"
    scp.RemotePathTransformation = RemotePathTransformation.ShellQuote;
    events: Uploading, Downloading

PORT FORWARDING            (Connect -> AddForwardedPort -> Start)
    new ForwardedPortLocal([boundHost, ]boundPort, host, port)      // ssh -L
    new ForwardedPortRemote([boundHost, ]boundPort, host, port)     // ssh -R
    new ForwardedPortDynamic([host, ]port)                          // ssh -D (SOCKS)
    client.AddForwardedPort(p); p.Start(); ... p.Stop(); client.RemoveForwardedPort(p);
    p.IsStarted   p.BoundPort   events: Exception, RequestReceived, Closing

NETCONF
    using var nc = new NetConfClient(host, user, password);
    nc.SendReceiveRpc(xmlOrDocument)   nc.SendCloseRpc()
    nc.ServerCapabilities   nc.ClientCapabilities   nc.AutomaticMessageIdHandling

DIAGNOSTICS
    SshNetLoggingConfiguration.InitializeLogging(loggerFactory);   // process-wide
    connectionInfo.LoggerFactory = loggerFactory;                  // per connection
    client.ErrorOccurred / ServerIdentificationReceived

EXCEPTIONS  (all under CodeBrix.SSH.Common, all derive from SshException)
    SshConnectionException (.DisconnectReason)   SshAuthenticationException
    SshOperationTimeoutException                 SshPassPhraseNullOrEmptyException
    SftpException (.StatusCode) -> SftpPathNotFoundException, SftpPermissionDeniedException
    ScpException   ProxyException   NetConfServerException

MOCKING
    ISshClient / ISftpClient / IBaseClient are the seams. ScpClient and
    NetConfClient have no interface.

================================================================================
