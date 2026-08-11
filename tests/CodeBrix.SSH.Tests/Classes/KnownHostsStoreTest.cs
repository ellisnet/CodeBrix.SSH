using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CodeBrix.SSH.KnownHosts;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes;

public class KnownHostsStoreTest : IDisposable
{
    private static readonly byte[] KeyA = Encoding.ASCII.GetBytes("key-a-bytes-000000000001");
    private static readonly byte[] KeyB = Encoding.ASCII.GetBytes("key-b-bytes-000000000002");

    private readonly string _tempDirectory;

    public KnownHostsStoreTest()
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

    private string WriteKnownHostsFile(string newLine, params string[] lines)
    {
        var path = Path.Combine(_tempDirectory, Path.GetRandomFileName());
        File.WriteAllText(path, string.Join(newLine, lines) + newLine);
        return path;
    }

    private static string ToBase64(byte[] key)
    {
        return Convert.ToBase64String(key);
    }

    [Fact]
    public void Verify_PlainHostEntry_ReturnsKnown()
    {
        var path = WriteKnownHostsFile("\n", "example.com ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Known, store.Verify("example.com", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void Verify_UnknownHost_ReturnsUnknown()
    {
        var path = WriteKnownHostsFile("\n", "example.com ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Unknown, store.Verify("other.com", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void Verify_SameHostAndAlgorithmDifferentKey_ReturnsMismatch()
    {
        var path = WriteKnownHostsFile("\n", "example.com ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Mismatch, store.Verify("example.com", 22, "ssh-ed25519", KeyB));
    }

    [Fact]
    public void Verify_DifferentAlgorithm_ReturnsUnknown()
    {
        var path = WriteKnownHostsFile("\n", "example.com ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Unknown, store.Verify("example.com", 22, "ssh-rsa", KeyA));
    }

    [Fact]
    public void Verify_HostMatching_IsCaseInsensitive()
    {
        var path = WriteKnownHostsFile("\n", "Example.COM ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Known, store.Verify("EXAMPLE.com", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void Verify_NonDefaultPort_UsesBracketedForm()
    {
        var path = WriteKnownHostsFile("\n", "[example.com]:2222 ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Known, store.Verify("example.com", 2222, "ssh-ed25519", KeyA));
        Assert.Equal(HostKeyVerificationResult.Unknown, store.Verify("example.com", 22, "ssh-ed25519", KeyA));
        Assert.Equal(HostKeyVerificationResult.Unknown, store.Verify("example.com", 2223, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void Verify_BracketedPort22Entry_MatchesDefaultPort()
    {
        var path = WriteKnownHostsFile("\n", "[example.com]:22 ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Known, store.Verify("example.com", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void Verify_CommaSeparatedHostList_MatchesEachName()
    {
        var path = WriteKnownHostsFile("\n", "example.com,alias.example.com,192.168.7.9 ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Known, store.Verify("alias.example.com", 22, "ssh-ed25519", KeyA));
        Assert.Equal(HostKeyVerificationResult.Known, store.Verify("192.168.7.9", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void Verify_WildcardPattern_Matches()
    {
        var path = WriteKnownHostsFile("\n", "*.example.com ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Known, store.Verify("foo.example.com", 22, "ssh-ed25519", KeyA));
        Assert.Equal(HostKeyVerificationResult.Unknown, store.Verify("example.com", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void Verify_NegatedPattern_ExcludesHostFromLine()
    {
        var path = WriteKnownHostsFile("\n", "*.example.com,!foo.example.com ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Known, store.Verify("bar.example.com", 22, "ssh-ed25519", KeyA));
        Assert.Equal(HostKeyVerificationResult.Unknown, store.Verify("foo.example.com", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void Verify_HashedEntry_Matches()
    {
        var salt = new byte[20];
        RandomNumberGenerator.Fill(salt);
        var hash = HMACSHA1.HashData(salt, Encoding.UTF8.GetBytes("example.com"));
        var pattern = "|1|" + Convert.ToBase64String(salt) + "|" + Convert.ToBase64String(hash);

        var path = WriteKnownHostsFile("\n", pattern + " ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Known, store.Verify("example.com", 22, "ssh-ed25519", KeyA));
        Assert.Equal(HostKeyVerificationResult.Unknown, store.Verify("other.com", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void Verify_RevokedEntry_ReturnsRevokedForExactKey()
    {
        var path = WriteKnownHostsFile(
            "\n",
            "example.com ssh-ed25519 " + ToBase64(KeyA),
            "@revoked example.com ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Revoked, store.Verify("example.com", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void Verify_RevokedEntryWithDifferentKey_DoesNotAffectResult()
    {
        var path = WriteKnownHostsFile("\n", "@revoked example.com ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Unknown, store.Verify("example.com", 22, "ssh-ed25519", KeyB));
    }

    [Fact]
    public void Verify_CertAuthorityLines_AreIgnored()
    {
        var path = WriteKnownHostsFile("\n", "@cert-authority example.com ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Unknown, store.Verify("example.com", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void Verify_CrlfLineEndings_AreAccepted()
    {
        var path = WriteKnownHostsFile("\r\n", "example.com ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Known, store.Verify("example.com", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void Verify_CommentsAndBlankLines_AreIgnored()
    {
        var path = WriteKnownHostsFile(
            "\n",
            "# a comment line",
            "",
            "   ",
            "example.com ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Known, store.Verify("example.com", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void MissingFile_IsTreatedAsEmptyStore()
    {
        var path = Path.Combine(_tempDirectory, "does-not-exist");
        var store = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Unknown, store.Verify("example.com", 22, "ssh-ed25519", KeyA));
    }

    [Fact]
    public void Constructor_NullOrWhiteSpacePath_Throws()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new KnownHostsStore(null));
        _ = Assert.Throws<ArgumentException>(() => new KnownHostsStore("   "));
    }

    [Fact]
    public void AddAndSave_RoundTrips()
    {
        var path = Path.Combine(_tempDirectory, "known_hosts");
        var store = new KnownHostsStore(path);

        store.Add("example.com", 22, "ssh-ed25519", KeyA);
        store.Add("example.com", 2222, "ssh-rsa", KeyB);
        store.Save();

        var reloaded = new KnownHostsStore(path);

        Assert.Equal(HostKeyVerificationResult.Known, reloaded.Verify("example.com", 22, "ssh-ed25519", KeyA));
        Assert.Equal(HostKeyVerificationResult.Known, reloaded.Verify("example.com", 2222, "ssh-rsa", KeyB));

        var contents = File.ReadAllText(path);

        Assert.Contains("example.com ssh-ed25519 " + ToBase64(KeyA) + "\n", contents, StringComparison.Ordinal);
        Assert.Contains("[example.com]:2222 ssh-rsa " + ToBase64(KeyB) + "\n", contents, StringComparison.Ordinal);
    }

    [Fact]
    public void Save_PreservesCommentsAndUnknownLines()
    {
        var path = WriteKnownHostsFile(
            "\n",
            "# preserved comment",
            "not a parseable entry",
            "example.com ssh-ed25519 " + ToBase64(KeyA));
        var store = new KnownHostsStore(path);

        store.Add("other.com", 22, "ssh-ed25519", KeyB);
        store.Save();

        var contents = File.ReadAllText(path);

        Assert.Contains("# preserved comment\n", contents, StringComparison.Ordinal);
        Assert.Contains("not a parseable entry\n", contents, StringComparison.Ordinal);
        Assert.Contains("example.com ssh-ed25519 " + ToBase64(KeyA) + "\n", contents, StringComparison.Ordinal);
        Assert.Contains("other.com ssh-ed25519 " + ToBase64(KeyB) + "\n", contents, StringComparison.Ordinal);
    }

    [Fact]
    public void Save_CreatesDirectoryAndFile_WithOwnerOnlyPermissionsOffWindows()
    {
        var path = Path.Combine(_tempDirectory, "nested", ".ssh", "known_hosts");
        var store = new KnownHostsStore(path);

        store.Add("example.com", 22, "ssh-ed25519", KeyA);
        store.Save();

        Assert.True(File.Exists(path));

        if (!OperatingSystem.IsWindows())
        {
            Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute, File.GetUnixFileMode(Path.GetDirectoryName(path)));
            Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite, File.GetUnixFileMode(path));
        }
    }

    [Fact]
    public void DefaultFilePath_PointsToUserSshKnownHosts()
    {
        var expectedSuffix = Path.Combine(".ssh", "known_hosts");
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        Assert.EndsWith(expectedSuffix, KnownHostsStore.DefaultFilePath, StringComparison.Ordinal);
        Assert.StartsWith(userProfile, KnownHostsStore.DefaultFilePath, StringComparison.Ordinal);
    }
}
