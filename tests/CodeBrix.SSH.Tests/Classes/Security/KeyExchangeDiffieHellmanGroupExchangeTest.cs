using System;
using System.Security.Cryptography;
using CodeBrix.SSH.Security;
using Xunit;

namespace CodeBrix.SSH.Tests.Classes.Security; //was previously: Renci.SshNet.Tests.Classes.Security;

public class KeyExchangeDiffieHellmanGroupExchangeTest
{
    [Fact]
    public void NameShouldBeCtorValue()
    {
        KeyExchangeDiffieHellmanGroupExchange kex = new("diffie-hellman-group-exchange-sha256", HashAlgorithmName.SHA512);

        Assert.Equal("diffie-hellman-group-exchange-sha256", kex.Name);
    }

    [Fact]
    public void Ctor_ArgumentNullException()
    {
        var ex = Assert.ThrowsAny<ArgumentNullException>(() => new KeyExchangeDiffieHellmanGroupExchange(name: null, HashAlgorithmName.SHA512));
        Assert.Equal("name", ex.ParamName);

        ex = Assert.ThrowsAny<ArgumentNullException>(() => new KeyExchangeDiffieHellmanGroupExchange("kex", default));
        Assert.Equal("hashAlgorithm", ex.ParamName);

        ex = Assert.ThrowsAny<ArgumentNullException>(() => new KeyExchangeDiffieHellmanGroupExchange("kex", new HashAlgorithmName(null)));
        Assert.Equal("hashAlgorithm", ex.ParamName);
    }

    [Fact]
    public void Ctor_InvalidHashAlgorithm_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new KeyExchangeDiffieHellmanGroupExchange("kex", new HashAlgorithmName("bad")));
        Assert.Equal("hashAlgorithm", ex.ParamName);

        ex = Assert.Throws<ArgumentException>(() => new KeyExchangeDiffieHellmanGroupExchange("kex", new HashAlgorithmName("")));
        Assert.Equal("hashAlgorithm", ex.ParamName);
    }

    [Fact]
    public void Ctor_InvalidGroupSizes_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new KeyExchangeDiffieHellmanGroupExchange("kex", HashAlgorithmName.SHA512, 1024, 4096, 2048));
        Assert.Equal("preferredGroupSize", ex.ParamName);

        ex = Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new KeyExchangeDiffieHellmanGroupExchange("kex", HashAlgorithmName.SHA512, 8192, 4096, 2048));
        Assert.Equal("preferredGroupSize", ex.ParamName);
    }
}
