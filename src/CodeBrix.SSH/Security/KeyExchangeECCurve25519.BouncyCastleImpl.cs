using CodeBrix.Cryptography.Crypto.Agreement;
using CodeBrix.Cryptography.Crypto.Generators;
using CodeBrix.Cryptography.Crypto.Parameters;
using CodeBrix.SSH.Abstractions;

namespace CodeBrix.SSH.Security; //was previously: Renci.SshNet.Security;

internal partial class KeyExchangeECCurve25519
{
    protected sealed class BouncyCastleImpl : Impl
    {
        private X25519Agreement _keyAgreement;

        public override byte[] GenerateClientPublicKey()
        {
            var g = new X25519KeyPairGenerator();
            g.Init(new X25519KeyGenerationParameters(CryptoAbstraction.SecureRandom));

            var aKeyPair = g.GenerateKeyPair();
            _keyAgreement = new X25519Agreement();
            _keyAgreement.Init(aKeyPair.Private);

            return ((X25519PublicKeyParameters)aKeyPair.Public).GetEncoded();
        }

        public override byte[] CalculateAgreement(byte[] serverPublicKey)
        {
            var publicKey = new X25519PublicKeyParameters(serverPublicKey);

            var k1 = new byte[_keyAgreement.AgreementSize];
            _keyAgreement.CalculateAgreement(publicKey, k1, 0);

            return k1;
        }
    }
}
