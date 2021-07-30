using System.Collections.Generic;
using Mirage;
using Mirage.Logging;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using UnityEngine;

namespace BlackBox
{
    public class BlackBoxFactory : MonoBehaviour
    {
        #region Network Messages

        [NetworkMessage]
        private struct SharePublicKey
        {
            public string PublicShareKeyX;
            public string PublicShareKeyY;
        }

        #endregion

        #region Fields

        private static readonly ILogger Logger = LogFactory.GetLogger(typeof(BlackBoxFactory));

        public BlackBoxEncryption BlackBoxEncryption;
        private NetworkServer _server;
        private NetworkClient _client;

        private Dictionary<INetworkPlayer, AsymmetricCipherKeyPair> _clientKeys = new Dictionary<INetworkPlayer, AsymmetricCipherKeyPair>();

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _server = GetComponent<NetworkServer>();
            _client = GetComponent<NetworkClient>();

            _server.Started.AddListener(OnServerStarted);

            _client.Started.AddListener(OnClientStarted);
        }

        #endregion

        #region Client

        /// <summary>
        ///     Client has started up let's assign client handler to encryption.
        /// </summary>
        private void OnClientStarted()
        {
            // Calculate client's key pair.
            BlackBoxEncryption ??= new BlackBoxEncryption(_client.MessageHandler);

            _client.MessageHandler.RegisterHandler<SharePublicKey>(OnServerSharePublicKey);
        }

        /// <summary>
        ///     Received message from server with server's public key.
        /// </summary>
        /// <param name="player">The player that is receiving this message.</param>
        /// <param name="message">The message data we received.</param>
        private void OnServerSharePublicKey(INetworkPlayer player, SharePublicKey message)
        {
            var publicKey = BlackBoxEncryption.KeyPair.Public as ECPublicKeyParameters;

            player.Send(new SharePublicKey
            {
                PublicShareKeyX = publicKey?.Q.AffineXCoord.ToBigInteger().ToString(),
                PublicShareKeyY = publicKey?.Q.AffineYCoord.ToBigInteger().ToString()
            });

            ECPoint point = BlackBoxEncryption.X9EC.Curve.CreatePoint(new BigInteger(message.PublicShareKeyX),
                new BigInteger(message.PublicShareKeyY));

            // Calculate shared key from server's public key.
            BlackBoxEncryption.AesKey =
                BlackBoxEncryption.GenerateAesKey(new ECPublicKeyParameters("ECDH", point,
                    SecObjectIdentifiers.SecP521r1));
        }

        #endregion

        #region Server

        /// <summary>
        ///     Server has started up let's assign server handler now to encryption.
        /// </summary>
        private void OnServerStarted()
        {
            // Create server key pair.
            BlackBoxEncryption ??= new BlackBoxEncryption(_server.MessageHandler);

            _server.MessageHandler.RegisterHandler<SharePublicKey>(OnClientSharePublicKey);
            _server.Connected.AddListener(OnServerAuthenticated);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        private void OnServerAuthenticated(INetworkPlayer player)
        {
            var publicKey = BlackBoxEncryption.KeyPair.Public as ECPublicKeyParameters;

            // Send client the server's public key.
            player.Send(new SharePublicKey
            {
                PublicShareKeyX = publicKey?.Q.AffineXCoord.ToBigInteger().ToString(),
                PublicShareKeyY = publicKey?.Q.AffineYCoord.ToBigInteger().ToString()
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        private void OnClientSharePublicKey(INetworkPlayer player, SharePublicKey message)
        {
            ECPoint point = BlackBoxEncryption.X9EC.Curve.CreatePoint(new BigInteger(message.PublicShareKeyX),
                new BigInteger(message.PublicShareKeyY));

            // Calculate shared key from server's public key.
            BlackBoxEncryption.AesKey =
                BlackBoxEncryption.GenerateAesKey(new ECPublicKeyParameters("ECDH", point,
                    SecObjectIdentifiers.SecP521r1));
        }

        #endregion
    }
}
