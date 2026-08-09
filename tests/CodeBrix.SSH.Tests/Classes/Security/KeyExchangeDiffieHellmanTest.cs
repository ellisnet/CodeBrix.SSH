using System;
using System.Security.Cryptography;
using CodeBrix.Cryptography.Crypto.Agreement;
using CodeBrix.SSH.Security;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Security; //was previously: Renci.SshNet.Tests.Classes.Security;

public class KeyExchangeDiffieHellmanTest
{
    [Fact]
    public void NameShouldBeCtorValue()
    {
        KeyExchangeDiffieHellman kex = new("diffie-hellman-group16-sha512", DHStandardGroups.rfc3526_4096, HashAlgorithmName.SHA512);

        Assert.Equal("diffie-hellman-group16-sha512", kex.Name);
    }

    [Fact]
    public void Ctor_ArgumentNullException()
    {
        var ex = Assert.ThrowsAny<ArgumentNullException>(() => new KeyExchangeDiffieHellman(name: null, DHStandardGroups.rfc3526_4096, HashAlgorithmName.SHA512));
        Assert.Equal("name", ex.ParamName);

        ex = Assert.ThrowsAny<ArgumentNullException>(() => new KeyExchangeDiffieHellman("kex", parameters: null, HashAlgorithmName.SHA512));
        Assert.Equal("parameters", ex.ParamName);

        ex = Assert.ThrowsAny<ArgumentNullException>(() => new KeyExchangeDiffieHellman("kex", DHStandardGroups.rfc3526_4096, default));
        Assert.Equal("hashAlgorithm", ex.ParamName);

        ex = Assert.ThrowsAny<ArgumentNullException>(() => new KeyExchangeDiffieHellman("kex", DHStandardGroups.rfc3526_4096, new HashAlgorithmName(null)));
        Assert.Equal("hashAlgorithm", ex.ParamName);
    }

    [Fact]
    public void Ctor_InvalidHashAlgorithm_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new KeyExchangeDiffieHellman("kex", DHStandardGroups.rfc3526_4096, new HashAlgorithmName("bad")));
        Assert.Equal("hashAlgorithm", ex.ParamName);

        ex = Assert.Throws<ArgumentException>(() => new KeyExchangeDiffieHellman("kex", DHStandardGroups.rfc3526_4096, new HashAlgorithmName("")));
        Assert.Equal("hashAlgorithm", ex.ParamName);
    }
}
