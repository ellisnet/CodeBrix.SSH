# CodeBrix.SSH

A fully managed, cross-platform Secure Shell (SSH-2) library for .NET, providing SSH command execution, an interactive shell, SFTP, SCP, and local/remote/dynamic port forwarding.
CodeBrix.SSH is a .NET 10 fork of [SSH.NET](https://github.com/sshnet/SSH.NET) 2025.1.0, carrying the same public API surface under the `CodeBrix.SSH` namespace, and is provided as a .NET 10 library and associated `CodeBrix.SSH.MitLicenseForever` NuGet package.

CodeBrix.SSH supports applications and assemblies that target Microsoft .NET version 10.0 and later.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

## Migrating from SSH.NET

Every public type keeps its SSH.NET name — only the root namespace changes. In most projects the migration is a package swap and a find-and-replace of the `using` directives:

```csharp
using Renci.SshNet;         // before
using CodeBrix.SSH;         // after

using Renci.SshNet.Sftp;    // before
using CodeBrix.SSH.Sftp;    // after
```

The sub-namespaces map one-to-one: `Renci.SshNet.X` becomes `CodeBrix.SSH.X` for `Common`, `Sftp`, `Security`, `Messages`, `Channels`, `Connection`, `Compression`, `Abstractions` and `NetConf`.

CodeBrix.SSH also carries six fixes made upstream after the 2025.1.0 release, so its public surface is a small superset of SSH.NET 2025.1.0:

* `ScpClient.Download` truncates the local file instead of leaving stale trailing bytes when overwriting a larger existing file
* `ScpClient.UseDirectoryFlag` opts out of the undocumented `-d` scp flag, for servers (such as Cisco) that reject it
* `SftpClient.UploadFileAsync` gained a `canOverride` overload
* `SftpClient.DownloadFileAsync` and `UploadFileAsync` accept an `IProgress<DownloadFileProgressReport>` / `IProgress<UploadFileProgressReport>`

Two behavioural notes:

* The SSH protocol version-exchange string this client sends is now `SSH-2.0-CodeBrix.SSH.SshClient.<version>` rather than `SSH-2.0-Renci.SshNet.SshClient.<version>`.
* Logger categories are derived from type names, so they are now rooted at `CodeBrix.SSH.*`.

## CodeBrix.SSH supports:

* Execution of SSH commands using both synchronous and asynchronous methods
* SFTP functionality for both synchronous and asynchronous operations
* SCP functionality
* Remote, dynamic and local port forwarding
* Interactive shell/terminal implementation
* Authentication via public key, password and keyboard-interactive methods, including multi-factor
* Connection via SOCKS4, SOCKS5 or HTTP proxy

## Sample Code

### Run a command

```csharp
using CodeBrix.SSH;

using (var client = new SshClient("sftp.foo.com", "guest", new PrivateKeyFile("path/to/my/key")))
{
    client.Connect();
    using SshCommand cmd = client.RunCommand("echo 'Hello World!'");
    Console.WriteLine(cmd.Result); // "Hello World!\n"
}
```

### Upload and list files using SFTP

```csharp
using CodeBrix.SSH;
using CodeBrix.SSH.Sftp;

using (var client = new SftpClient("sftp.foo.com", "guest", "pwd"))
{
    client.Connect();

    using (FileStream fs = File.OpenRead("/tmp/test-file.txt"))
    {
        client.UploadFile(fs, "/home/guest/test-file.txt");
    }

    foreach (ISftpFile file in client.ListDirectory("/home/guest/"))
    {
        Console.WriteLine($"{file.FullName} {file.LastWriteTime}");
    }
}
```

## Main Types

The main types provided by this library are:

* `CodeBrix.SSH.SshClient`
* `CodeBrix.SSH.SftpClient`
* `CodeBrix.SSH.ScpClient`
* `CodeBrix.SSH.PrivateKeyFile`
* `CodeBrix.SSH.SshCommand`
* `CodeBrix.SSH.ForwardedPort`
* `CodeBrix.SSH.ShellStream`

## Encryption Methods

CodeBrix.SSH supports the following encryption methods:
* aes128-ctr
* aes192-ctr
* aes256-ctr
* aes128-gcm<span></span>@openssh.com
* aes256-gcm<span></span>@openssh.com
* chacha20-poly1305<span></span>@openssh.com
* aes128-cbc
* aes192-cbc
* aes256-cbc
* 3des-cbc

## Key Exchange Methods

CodeBrix.SSH supports the following key exchange methods:
* mlkem768x25519-sha256
* sntrup761x25519-sha512
* sntrup761x25519-sha512<span></span>@openssh.com
* curve25519-sha256
* curve25519-sha256<span></span>@libssh.org
* ecdh-sha2-nistp256
* ecdh-sha2-nistp384
* ecdh-sha2-nistp521
* diffie-hellman-group-exchange-sha256
* diffie-hellman-group-exchange-sha1
* diffie-hellman-group16-sha512
* diffie-hellman-group14-sha256
* diffie-hellman-group14-sha1
* diffie-hellman-group1-sha1

## Public Key Authentication

CodeBrix.SSH supports the following private key formats:
* RSA in
  * OpenSSL traditional PEM format ("BEGIN RSA PRIVATE KEY")
  * OpenSSL PKCS#8 PEM format ("BEGIN PRIVATE KEY", "BEGIN ENCRYPTED PRIVATE KEY")
  * ssh.com format ("BEGIN SSH2 ENCRYPTED PRIVATE KEY")
  * OpenSSH key format ("BEGIN OPENSSH PRIVATE KEY")
  * PuTTY private key format ("PuTTY-User-Key-File-2", "PuTTY-User-Key-File-3")
* ECDSA 256/384/521 in
  * OpenSSL traditional PEM format ("BEGIN EC PRIVATE KEY")
  * OpenSSL PKCS#8 PEM format ("BEGIN PRIVATE KEY", "BEGIN ENCRYPTED PRIVATE KEY")
  * OpenSSH key format ("BEGIN OPENSSH PRIVATE KEY")
  * PuTTY private key format ("PuTTY-User-Key-File-2", "PuTTY-User-Key-File-3")
* ED25519 in
  * OpenSSL PKCS#8 PEM format ("BEGIN PRIVATE KEY", "BEGIN ENCRYPTED PRIVATE KEY")
  * OpenSSH key format ("BEGIN OPENSSH PRIVATE KEY")
  * PuTTY private key format ("PuTTY-User-Key-File-2", "PuTTY-User-Key-File-3")

Private keys in OpenSSL traditional PEM format can be encrypted using one of the following cipher methods:
* DES-EDE3-CBC
* DES-EDE3-CFB
* AES-128-CBC
* AES-192-CBC
* AES-256-CBC

Private keys in OpenSSL PKCS#8 PEM format can be encrypted using any cipher method CodeBrix.Cryptography supports.

Private keys in ssh.com format can be encrypted using the following cipher method:
* 3des-cbc

Private keys in OpenSSH key format can be encrypted using one of the following cipher methods:
* 3des-cbc
* aes128-cbc
* aes192-cbc
* aes256-cbc
* aes128-ctr
* aes192-ctr
* aes256-ctr
* aes128-gcm<span></span>@openssh.com
* aes256-gcm<span></span>@openssh.com
* chacha20-poly1305<span></span>@openssh.com

Private keys in PuTTY private key format can be encrypted using the following cipher method:
* aes256-cbc

## Host Key Algorithms

CodeBrix.SSH supports the following host key algorithms:
* ssh-ed25519
* ecdsa-sha2-nistp256
* ecdsa-sha2-nistp384
* ecdsa-sha2-nistp521
* rsa-sha2-512
* rsa-sha2-256
* ssh-rsa

OpenSSH certificate authentication is supported for all of the above, e.g. ssh-ed25519-cert-v01<span></span>@openssh.com.

## Message Authentication Code

CodeBrix.SSH supports the following MAC algorithms:
* hmac-sha2-256
* hmac-sha2-512
* hmac-sha1
* hmac-sha2-256-etm<span></span>@openssh.com
* hmac-sha2-512-etm<span></span>@openssh.com
* hmac-sha1-etm<span></span>@openssh.com

## Compression

CodeBrix.SSH supports the following compression algorithms:
* none (default)
* zlib<span></span>@openssh.com

## License

The project is licensed under the MIT License. see: https://en.wikipedia.org/wiki/MIT_License
