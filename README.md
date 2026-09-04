# CodeBrix.SSH

A fully managed, cross-platform Secure Shell (SSH-2) library for .NET, providing SSH command execution, an interactive shell, SFTP, SCP, and local/remote/dynamic port forwarding.
CodeBrix.SSH is provided as a .NET 10 library and associated `CodeBrix.SSH.MitLicenseForever` NuGet package.

CodeBrix.SSH supports applications and assemblies that target Microsoft .NET version 10.0 and later.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

## Installation

```
dotnet add package CodeBrix.SSH.MitLicenseForever
```

Note that the NuGet package ID and the namespace are different - there is no package named plain `CodeBrix.SSH`:

* NuGet package ID: `CodeBrix.SSH.MitLicenseForever`
* Assembly and primary namespace: `CodeBrix.SSH` - i.e. `using CodeBrix.SSH;`

The sub-namespaces are `CodeBrix.SSH.Abstractions`, `.Channels`, `.Common`, `.Compression`, `.Connection`, `.KnownHosts`, `.Messages`, `.NetConf`, `.Security` and `.Sftp`.

XML documentation (IntelliSense) ships alongside the assembly.

The package pulls in the following automatically; no version pinning is needed in the consuming project:

* `CodeBrix.Cryptography.MitLicenseForever` - the cryptographic primitives
* `Microsoft.Extensions.Logging.Abstractions` - the logging seam

Two things worth knowing before you connect:

* The SSH protocol version-exchange string this client sends is `SSH-2.0-CodeBrix.SSH.SshClient.<version>`. A server that filters on the client identification string has to allow it.
* Logger categories are derived from type names, so they are rooted at `CodeBrix.SSH.*`.

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
* diffie-hellman-group16-sha512
* diffie-hellman-group18-sha512
* diffie-hellman-group14-sha256
* diffie-hellman-group-exchange-sha1
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

## Documentation

The NuGet package includes `AGENT-README.txt`, a complete API reference and usage guide written for AI coding agents - point your agent at that file when it is writing code against this library.

Additional sample code and usage examples are available in the `CodeBrix.SSH.Tests` project:
https://github.com/ellisnet/CodeBrix.SSH/tree/main/tests/CodeBrix.SSH.Tests

## License

CodeBrix.SSH is licensed under the MIT License - see the
[LICENSE](https://github.com/ellisnet/CodeBrix.SSH/blob/main/LICENSE) file.

For licensing and provenance information about the open source code included in
this package, see [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.SSH/blob/main/THIRD-PARTY-NOTICES.txt).
