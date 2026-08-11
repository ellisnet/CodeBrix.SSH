using System;
using System.IO;
using System.Text;
using CodeBrix.SSH.KnownHosts;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes;

public class KnownHostsClientExtensionsTest : IDisposable
{
    private static readonly byte[] KeyA = Encoding.ASCII.GetBytes("key-a-bytes-000000000001");
    private static readonly byte[] KeyB = Encoding.ASCII.GetBytes("key-b-bytes-000000000002");

    private readonly string _tempDirectory;

    public KnownHostsClientExtensionsTest()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "CodeBrix.SSH.Tests_" + Path.GetRandomFileName());
        _ = Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private KnownHostsStore CreateStore(params string[] lines)
    {
        var path = Path.Combine(_tempDirectory, Path.GetRandomFileName());

        if (lines.Length > 0)
        {
            File.WriteAllText(path, string.Join('\n', lines) + "\n");
        }

        return new KnownHostsStore(path);
    }

    private static string ToBase64(byte[] key)
    {
        return Convert.ToBase64String(key);
    }

    [Fact]
    public void DecideTrust_KnownKey_ReturnsTrue()
    {
        var store = CreateStore("example.com ssh-ed25519 " + ToBase64(KeyA));

        var trusted = KnownHostsClientExtensions.DecideTrust(store, "example.com", 22, "ssh-ed25519", KeyA, "fp", trustOnFirstUse: false, mismatchCallback: null);

        Assert.True(trusted);
    }

    [Fact]
    public void DecideTrust_UnknownKey_Strict_ReturnsFalseAndDoesNotModifyStore()
    {
        var store = CreateStore();

        var trusted = KnownHostsClientExtensions.DecideTrust(store, "example.com", 22, "ssh-ed25519", KeyA, "fp", trustOnFirstUse: false, mismatchCallback: null);

        Assert.False(trusted);
        Assert.False(File.Exists(store.FilePath));
        Assert.Equal(HostKeyVerificationResult.Unknown, store.Verify("example.com", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void DecideTrust_UnknownKey_TrustOnFirstUse_ReturnsTrueAndPersists()
    {
        var store = CreateStore();

        var trusted = KnownHostsClientExtensions.DecideTrust(store, "example.com", 2222, "ssh-ed25519", KeyA, "fp", trustOnFirstUse: true, mismatchCallback: null);

        Assert.True(trusted);
        Assert.True(File.Exists(store.FilePath));

        var reloaded = new KnownHostsStore(store.FilePath);

        Assert.Equal(HostKeyVerificationResult.Known, reloaded.Verify("example.com", 2222, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void DecideTrust_Mismatch_NoCallback_ReturnsFalse()
    {
        var store = CreateStore("example.com ssh-ed25519 " + ToBase64(KeyA));

        var trusted = KnownHostsClientExtensions.DecideTrust(store, "example.com", 22, "ssh-ed25519", KeyB, "fp", trustOnFirstUse: true, mismatchCallback: null);

        Assert.False(trusted);
    }

    [Fact]
    public void DecideTrust_Mismatch_CallbackAccepts_ReturnsTrueAndDoesNotModifyStore()
    {
        var store = CreateStore("example.com ssh-ed25519 " + ToBase64(KeyA));

        var trusted = KnownHostsClientExtensions.DecideTrust(store, "example.com", 22, "ssh-ed25519", KeyB, "fp", trustOnFirstUse: true, mismatchCallback: info => true);

        Assert.True(trusted);
        Assert.Equal(HostKeyVerificationResult.Mismatch, store.Verify("example.com", 22, "ssh-ed25519", KeyB));
    }

    [Fact]
    public void DecideTrust_Mismatch_CallbackReceivesDetails()
    {
        var store = CreateStore("[example.com]:2222 ssh-ed25519 " + ToBase64(KeyA));

        HostKeyMismatchInfo received = null;

        _ = KnownHostsClientExtensions.DecideTrust(store, "example.com", 2222, "ssh-ed25519", KeyB, "the-fingerprint", trustOnFirstUse: true, mismatchCallback: info =>
        {
            received = info;
            return false;
        });

        Assert.NotNull(received);
        Assert.Equal("example.com", received.Host);
        Assert.Equal(2222, received.Port);
        Assert.Equal("ssh-ed25519", received.KeyAlgorithm);
        Assert.Equal(KeyB, received.HostKey);
        Assert.Equal("the-fingerprint", received.FingerPrintSHA256);
        Assert.Equal(HostKeyVerificationResult.Mismatch, received.Result);
    }

    [Fact]
    public void DecideTrust_RevokedKey_ReturnsFalseAndReportsRevoked()
    {
        var store = CreateStore("@revoked example.com ssh-ed25519 " + ToBase64(KeyA));

        HostKeyMismatchInfo received = null;

        var trustedWithoutCallback = KnownHostsClientExtensions.DecideTrust(store, "example.com", 22, "ssh-ed25519", KeyA, "fp", trustOnFirstUse: true, mismatchCallback: null);
        var trustedWithCallback = KnownHostsClientExtensions.DecideTrust(store, "example.com", 22, "ssh-ed25519", KeyA, "fp", trustOnFirstUse: true, mismatchCallback: info =>
        {
            received = info;
            return false;
        });

        Assert.False(trustedWithoutCallback);
        Assert.False(trustedWithCallback);
        Assert.NotNull(received);
        Assert.Equal(HostKeyVerificationResult.Revoked, received.Result);
    }

    [Fact]
    public void UseHelpers_NullArguments_Throw()
    {
        var store = CreateStore();

        using var client = new SshClient("example.com", "user", "password");

        _ = Assert.Throws<ArgumentNullException>(() => ((SshClient)null).UseStrictHostKeyVerification(store));
        _ = Assert.Throws<ArgumentNullException>(() => client.UseStrictHostKeyVerification(null));
        _ = Assert.Throws<ArgumentNullException>(() => ((SshClient)null).UseTrustOnFirstUse(store));
        _ = Assert.Throws<ArgumentNullException>(() => client.UseTrustOnFirstUse(null));
        _ = Assert.Throws<ArgumentNullException>(() => client.UseTrustOnFirstUse(store, mismatchCallback: null));
    }

    [Fact]
    public void UseHelpers_SubscribeWithoutConnecting()
    {
        var store = CreateStore();

        using var client = new SshClient("example.com", "user", "password");

        client.UseStrictHostKeyVerification(store);
        client.UseTrustOnFirstUse(store);
        client.UseTrustOnFirstUse(store, info => false);
    }
}
