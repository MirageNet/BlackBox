using System;
using Mirage;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using UnityEngine;
using UnityEngine.Events;

namespace BlackBox
{
    [Serializable]
    public class GeneratedPublicKeyEvent : UnityEvent<INetworkPlayer> { }

    public class BlackBoxFactory : MonoBehaviour
    {
        #region Network Messages

        [NetworkMessage]
        internal struct SharePublicKey
        {
            public string PublicShareKeyX;
            public string PublicShareKeyY;
        }

        #endregion

        #region Fields

        public BlackBoxEncryption ClientBlackBoxEncryption;
        public BlackBoxEncryption ServerBlackBoxEncryption;
        private NetworkServer _server;
        private NetworkClient _client;

        #region Events

        [Header("Events")] [SerializeField]
        private GeneratedPublicKeyEvent _serverGeneratedSharedKey = new GeneratedPublicKeyEvent();
        private GeneratedPublicKeyEvent _clientGeneratedSharedKey = new GeneratedPublicKeyEvent();

        public GeneratedPublicKeyEvent OnServerGeneratedSharedKey => _serverGeneratedSharedKey;

        public GeneratedPublicKeyEvent OnClientGeneratedSharedKey => _clientGeneratedSharedKey;

        #endregion

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _server = GetComponent<NetworkServer>();
            _client = GetComponent<NetworkClient>();

            _server.Started.AddListener(OnServerStarted);
            _server.Connected.AddListener(OnServerAuthenticated);
            _server.Stopped.AddListener(OnServerStop);
            _server.Disconnected.AddListener(OnServerPlayerDisconnected);

            _client.Started.AddListener(OnClientStarted);
            _client.Disconnected.AddListener(OnClientDisconnected);
        }

        #endregion

        #region Client

        private void OnClientDisconnected(ClientStoppedReason error)
        {
            ClientBlackBoxEncryption = null;
        }

        /// <summary>
        ///     Client has started up let's assign client handler to encryption.
        /// </summary>
        private void OnClientStarted()
        {
            // Calculate client's key pair.
            ClientBlackBoxEncryption = new BlackBoxEncryption(_client.MessageHandler);

            _client.MessageHandler.RegisterHandler<SharePublicKey>(OnServerSharePublicKey);
        }

        /// <summary>
        ///     Received message from server with server's public key.
        /// </summary>
        /// <param name="player">The player that is receiving this message.</param>
        /// <param name="message">The message data we received.</param>
        private void OnServerSharePublicKey(INetworkPlayer player, SharePublicKey message)
        {
            var publicKey = ClientBlackBoxEncryption.KeyPair.Public as ECPublicKeyParameters;

            player.Send(new SharePublicKey
            {
                PublicShareKeyX = publicKey?.Q.AffineXCoord.ToBigInteger().ToString(),
                PublicShareKeyY = publicKey?.Q.AffineYCoord.ToBigInteger().ToString()
            });

            ECPoint point = ClientBlackBoxEncryption.X9EC.Curve.CreatePoint(new BigInteger(message.PublicShareKeyX),
                new BigInteger(message.PublicShareKeyY));

            // Calculate shared key from server's public key.
            ClientBlackBoxEncryption.GenerateAesKey(player,
                new ECPublicKeyParameters("ECDH", point, SecObjectIdentifiers.SecP521r1));

            _clientGeneratedSharedKey.Invoke(player);
        }

        #endregion

        #region Server

        private void OnServerStop()
        {
            ServerBlackBoxEncryption = null;
        }

    /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        private void OnServerPlayerDisconnected(INetworkPlayer player)
        {
            ServerBlackBoxEncryption.RemoveKey(player);
        }

        /// <summary>
        ///     Server has started up let's assign server handler now to encryption.
        /// </summary>
        private void OnServerStarted()
        {
            // Create server key pair.
            ServerBlackBoxEncryption = new BlackBoxEncryption(_server.MessageHandler);

            _server.MessageHandler.RegisterHandler<SharePublicKey>(OnClientSharePublicKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        private void OnServerAuthenticated(INetworkPlayer player)
        {
            var publicKey = ServerBlackBoxEncryption.KeyPair.Public as ECPublicKeyParameters;

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
            ECPoint point = ServerBlackBoxEncryption.X9EC.Curve.CreatePoint(new BigInteger(message.PublicShareKeyX),
                new BigInteger(message.PublicShareKeyY));

            // Calculate shared key from client's public key.
            ServerBlackBoxEncryption.GenerateAesKey(player, new ECPublicKeyParameters("ECDH", point, SecObjectIdentifiers.SecP521r1));

            _serverGeneratedSharedKey.Invoke(player);
        }

        #endregion
    }
}
