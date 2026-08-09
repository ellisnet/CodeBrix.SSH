using System.Security.Cryptography;
using CodeBrix.Cryptography.Crypto.Prng;
using CodeBrix.Cryptography.Security;

namespace CodeBrix.SSH.Abstractions; //was previously: Renci.SshNet.Abstractions;

internal static class CryptoAbstraction
{
    private static readonly RandomNumberGenerator Randomizer = RandomNumberGenerator.Create();

    internal static readonly SecureRandom SecureRandom = new SecureRandom(new CryptoApiRandomGenerator(Randomizer));

    /// <summary>
    /// Generates a <see cref="byte"/> array of the specified length, and fills it with a
    /// cryptographically strong random sequence of values.
    /// </summary>
    /// <param name="length">The length of the array generate.</param>
    public static byte[] GenerateRandom(int length)
    {
        var random = new byte[length];
        Randomizer.GetBytes(random);
        return random;
    }

    public static byte[] HashMD5(byte[] source)
    {
        return MD5.HashData(source);
    }

    public static byte[] HashSHA1(byte[] source)
    {
        return SHA1.HashData(source);
    }

    public static byte[] HashSHA256(byte[] source)
    {
        return SHA256.HashData(source);
    }

    public static byte[] HashSHA384(byte[] source)
    {
        return SHA384.HashData(source);
    }

    public static byte[] HashSHA512(byte[] source)
    {
        return SHA512.HashData(source);
    }
}
