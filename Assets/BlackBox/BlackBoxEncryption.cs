using System;
using System.Security.Cryptography;
using Mirage;
using SecurityDriven.Inferno;
using SecurityDriven.Inferno.Extensions;

namespace BlackBox
{
    public class BlackBoxEncryption : Encryption
    {
        #region Fields

#if UNITY_EDITOR || UNITY_SERVER
        private CngKey _privateKeyStore;
#endif
        private readonly CngKey _publicKeyStore;
        private readonly SharedEphemeralBundle _ephemeralBundle;

        #endregion

        public BlackBoxEncryption(MessageHandler messageHandler) : base(messageHandler)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Create or open server key.
            _privateKeyStore = CngKey.Exists("ServerKey")
                ? CngKey.Open("ServerKey")
                : CngKeyExtensions.CreateNewDhmKey("ServerKey");
#endif
            _publicKeyStore = CngKey.Exists("ClientKey") ? CngKey.Open("ClientKey") : CngKeyExtensions.CreateNewDhmKey("ClientKey");

            _ephemeralBundle = _publicKeyStore.GetSharedEphemeralDhmSecret();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected override ArraySegment<byte> DecryptMessage(ArraySegment<byte> payload)
        {
            byte[] sharedSecret =
                _publicKeyStore.GetSharedDhmSecret(_ephemeralBundle.EphemeralDhmPublicKeyBlob.ToPublicKeyFromBlob());

            byte[] decrypted = SuiteB.Decrypt(sharedSecret, payload);

            return new ArraySegment<byte>(decrypted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected override ArraySegment<byte> EncryptMessage(ArraySegment<byte> payload)
        {
            byte[] encryptedData = SuiteB.Encrypt(_ephemeralBundle.SharedSecret, payload);

            return new ArraySegment<byte>(encryptedData);
        }
    }
}
