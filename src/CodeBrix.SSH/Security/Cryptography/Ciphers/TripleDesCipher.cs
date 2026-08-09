using System;
using System.Security.Cryptography;

namespace CodeBrix.SSH.Security.Cryptography.Ciphers; //was previously: Renci.SshNet.Security.Cryptography.Ciphers;

/// <summary>
/// Implements 3DES cipher algorithm.
/// </summary>
public sealed partial class TripleDesCipher : BlockCipher, IDisposable
{
    private readonly BclImpl _impl;

    /// <summary>
    /// Initializes a new instance of the <see cref="TripleDesCipher"/> class.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="iv">The IV.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="pkcs7Padding">Enable PKCS7 padding.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public TripleDesCipher(byte[] key, byte[] iv, System.Security.Cryptography.CipherMode mode, bool pkcs7Padding)
        : base(key, 8, mode: null, padding: null)
    {
        {
            _impl = new BclImpl(key, iv, mode, pkcs7Padding ? PaddingMode.PKCS7 : PaddingMode.None);
        }
    }

    /// <inheritdoc/>
    public override byte[] Encrypt(byte[] input, int offset, int length)
    {
        return _impl.Encrypt(input, offset, length);
    }

    /// <inheritdoc/>
    public override byte[] Decrypt(byte[] input, int offset, int length)
    {
        return _impl.Decrypt(input, offset, length);
    }

    /// <inheritdoc/>
    public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
    {
        return _impl.EncryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
    }

    /// <inheritdoc/>
    public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
    {
        return _impl.DecryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _impl.Dispose();
    }
}
