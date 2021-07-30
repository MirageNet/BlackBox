using System;
using Mirage;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using UnityEngine;

namespace BlackBox
{
    public class BlackBoxEncryption : Encryption
    {
        #region Fields

        internal readonly AsymmetricCipherKeyPair KeyPair;
        internal readonly X9ECParameters X9EC;
        internal byte[] AesKey;

        #endregion

        private AsymmetricCipherKeyPair GenerateKeyPair(ECDomainParameters ecDomain)
        {
            var keyGenerator = (ECKeyPairGenerator)GeneratorUtilities.GetKeyPairGenerator("ECDH");

            keyGenerator.Init(new ECKeyGenerationParameters(ecDomain, new SecureRandom()));

            return keyGenerator.GenerateKeyPair();
        }

        public byte[] GenerateAesKey(ECPublicKeyParameters bobPublicKey)
        {
            IBasicAgreement aKeyAgree = AgreementUtilities.GetBasicAgreement("ECDH");
            aKeyAgree.Init(KeyPair.Private);

            BigInteger sharedSecret = aKeyAgree.CalculateAgreement(bobPublicKey);
            byte[] sharedSecretBytes = sharedSecret.ToByteArray();

            IDigest digest = new Sha256Digest();
            byte[] symmetricKey = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(sharedSecretBytes, 0, sharedSecretBytes.Length);
            digest.DoFinal(symmetricKey, 0);

            return AesKey = symmetricKey;
        }

        public BlackBoxEncryption(MessageHandler messageHandler) : base(messageHandler)
        {
            X9EC = NistNamedCurves.GetByName("P-521");
            var ecDomain = new ECDomainParameters(X9EC.Curve, X9EC.G, X9EC.N, X9EC.H, X9EC.GetSeed());

            KeyPair = GenerateKeyPair(ecDomain);
        }

        private ArraySegment<byte> ProcessData(byte[] data, bool encrypt)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CFB/NOPADDING");

            cipher.Init(encrypt, new KeyParameter(AesKey));
            byte[] processed = new byte[cipher.GetOutputSize(data.Length)];

            int lengthProcess = cipher.ProcessBytes(data, 0, data.Length, processed, 0);

            byte[] processedData = cipher.DoFinal(processed, 0, lengthProcess);

            return new ArraySegment<byte>(processedData, 0, processedData.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected override ArraySegment<byte> DecryptMessage(ArraySegment<byte> payload)
        {
            Debug.Log($" Before Decrypted Length: {payload.Count} Message: {BitConverter.ToString(payload.Array)}");

            ArraySegment<byte> decrypted = ProcessData(payload.Array, false);

            Debug.Log($"After Decrypted Length: {decrypted.Count} Message {BitConverter.ToString(decrypted.Array)}");

            return decrypted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected override ArraySegment<byte> EncryptMessage(ArraySegment<byte> payload)
        {
            byte[] test = new byte[payload.Count];

            Array.Copy(payload.Array, test, test.Length);

            Debug.Log($" Before Encrypted  Length: {payload.Count} Message: {BitConverter.ToString(test)}");

            ArraySegment<byte> encryptedData = ProcessData(test, true);

            Debug.Log($"After Encrypted Length: {encryptedData.Count} Message {BitConverter.ToString(encryptedData.Array)}");

            return encryptedData;
        }
    }
}
