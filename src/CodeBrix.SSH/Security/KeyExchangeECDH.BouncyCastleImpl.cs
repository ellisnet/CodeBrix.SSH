using CodeBrix.Cryptography.Asn1.X9;
using CodeBrix.Cryptography.Crypto.Agreement;
using CodeBrix.Cryptography.Crypto.Generators;
using CodeBrix.Cryptography.Crypto.Parameters;
using CodeBrix.SSH.Abstractions;

namespace CodeBrix.SSH.Security; //was previously: Renci.SshNet.Security;

internal abstract partial class KeyExchangeECDH
{
    private sealed class BouncyCastleImpl : Impl
    {
        private readonly ECDomainParameters _domainParameters;
        private readonly ECDHCBasicAgreement _keyAgreement;

        public BouncyCastleImpl(X9ECParameters curveParameters)
        {
            _domainParameters = new ECDomainParameters(curveParameters);
            _keyAgreement = new ECDHCBasicAgreement();
        }

        public override byte[] GenerateClientPublicKey()
        {
            var g = new ECKeyPairGenerator();
            g.Init(new ECKeyGenerationParameters(_domainParameters, CryptoAbstraction.SecureRandom));

            var aKeyPair = g.GenerateKeyPair();
            _keyAgreement.Init(aKeyPair.Private);

            return ((ECPublicKeyParameters)aKeyPair.Public).Q.GetEncoded();
        }

        public override byte[] CalculateAgreement(byte[] serverPublicKey)
        {
            var c = _domainParameters.Curve;
            var q = c.DecodePoint(serverPublicKey);
            var publicKey = new ECPublicKeyParameters("ECDH", q, _domainParameters);

            return _keyAgreement.CalculateAgreement(publicKey).ToByteArray();
        }
    }
}
