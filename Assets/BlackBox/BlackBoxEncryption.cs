using System;
using System.Collections.Generic;
using Mirage;
using Mirage.Logging;
using Mirage.SocketLayer;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using UnityEngine;
using UnityEngine.Assertions;

namespace BlackBox
{
    public class BlackBoxEncryption : Encryption
    {
        #region Fields

        private static readonly ILogger Logger = LogFactory.GetLogger(typeof(BlackBoxEncryption));

        internal readonly AsymmetricCipherKeyPair KeyPair;
        internal readonly X9ECParameters X9EC;
        internal readonly Dictionary<IConnection, byte[]> ClientKeys = new Dictionary<IConnection, byte[]>();

        #endregion

        private AsymmetricCipherKeyPair GenerateKeyPair(ECDomainParameters ecDomain)
        {
            var keyGenerator = (ECKeyPairGenerator)GeneratorUtilities.GetKeyPairGenerator("ECDHC");

            keyGenerator.Init(new ECKeyGenerationParameters(ecDomain, new SecureRandom()));

            return keyGenerator.GenerateKeyPair();
        }

        /// <summary>
        ///     Generate a new shared public key using the public key from person sending us there public key.
        /// </summary>
        /// <param name="player">The player who shared us the public key.</param>
        /// <param name="sharedPublicKey">The public key to use to create a new shared public key.</param>
        public void GenerateAesKey(INetworkPlayer player, ECPublicKeyParameters sharedPublicKey)
        {
            IBasicAgreement aKeyAgree = AgreementUtilities.GetBasicAgreement("ECDHC");
            aKeyAgree.Init(KeyPair.Private);

            BigInteger sharedSecret = aKeyAgree.CalculateAgreement(sharedPublicKey);
            byte[] sharedSecretBytes = sharedSecret.ToByteArray();

            IDigest digest = new Sha256Digest();
            byte[] symmetricKey = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(sharedSecretBytes, 0, sharedSecretBytes.Length);
            digest.DoFinal(symmetricKey, 0);

            ClientKeys.Add(player.Connection, symmetricKey);
        }

        /// <summary>
        ///     Remove the player from our list so we don't store generated keys.
        /// </summary>
        /// <param name="player"></param>
        public void RemoveKey(INetworkPlayer player)
        {
            Assert.IsTrue(ClientKeys.Remove(player.Connection), "Player does not exist. Could not remove key from list.");
        }

        public BlackBoxEncryption(MessageHandler messageHandler) : base(messageHandler)
        {
            X9EC = NistNamedCurves.GetByName("P-521");
            var ecDomain = new ECDomainParameters(X9EC.Curve, X9EC.G, X9EC.N, X9EC.H, X9EC.GetSeed());

            KeyPair = GenerateKeyPair(ecDomain);
        }

        /// <summary>
        ///     Process the new data through here to allow the message to be encrypted or decrypted using
        ///     the correct public shared key we generate at runtime.
        /// </summary>
        /// <param name="player">The player that sent the message to be encrypted or decrypted.</param>
        /// <param name="data">The data we need to encrypt or decrypt.</param>
        /// <param name="encrypt">Whether or not we are going to encrypt or decrypt mode.</param>
        /// <returns></returns>
        private ArraySegment<byte> ProcessData(INetworkPlayer player, byte[] data, bool encrypt)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CFB/NOPADDING");

            cipher.Init(encrypt, ParameterUtilities.CreateKeyParameter("AES", ClientKeys[player.Connection]));

            byte[] processedData = cipher.DoFinal(data);

            return new ArraySegment<byte>(processedData, 0, processedData.Length);
        }

        /// <summary>
        ///     Decrypt the data before passing back to mirage.
        /// </summary>
        /// <param name="payload">The data that we want to decrypt.</param>
        /// <param name="player">The player we want to send encrypted data to.</param>
        /// <returns>Returns back a decrypted message.</returns>
        protected override ArraySegment<byte> DecryptMessage(INetworkPlayer player, ArraySegment<byte> payload)
        {
            byte[] strippedArray = new byte[payload.Count];

            Array.Copy(payload.Array, payload.Offset, strippedArray, 0, strippedArray.Length);

            Logger.Log($" Before Decrypted Length: {strippedArray.Length} Message: {BitConverter.ToString(strippedArray)}");

            ArraySegment<byte> decrypted = ProcessData(player, strippedArray, false);

            Logger.Log($"After Decrypted Length: {decrypted.Count} Message {BitConverter.ToString(decrypted.Array)}");

            return decrypted;
        }

        /// <summary>
        ///     Encrypt the data before sending it over the wire.
        /// </summary>
        /// <param name="payload">The data we want to encrypt.</param>
        /// <param name="player">The player we want to send encrypted data to.</param>
        /// <returns>Returns back new encrypted data.</returns>
        protected override ArraySegment<byte> EncryptMessage(INetworkPlayer player, ArraySegment<byte> payload)
        {
            byte[] strippedArray = new byte[payload.Count];

            Array.Copy(payload.Array, strippedArray, strippedArray.Length);

            Logger.Log($" Before Encrypted  Length: {payload.Count} Message: {BitConverter.ToString(strippedArray)}");

            ArraySegment<byte> encryptedData = ProcessData(player, strippedArray, true);

            Logger.Log($"After Encrypted Length: {encryptedData.Count} Message {BitConverter.ToString(encryptedData.Array)}");

            return encryptedData;
        }
    }
}
