using Mirage;
using Mirage.Logging;
using UnityEngine;

namespace BlackBox
{
    [RequireComponent(typeof(BlackBoxFactory))]
    public class BlackBoxLoginAuthenticator : NetworkAuthenticator
    {
        #region Network Messages

        [NetworkMessage]
        public struct AuthData
        {
            public string Username;
            public string Password;
        }

        [NetworkMessage]
        public struct AuthResponse
        {
            public bool Success;
            public string Message;
        }

        #endregion

        #region Fields

        private static readonly ILogger Logger = LogFactory.GetLogger(typeof(BlackBoxLoginAuthenticator));
        private BlackBoxFactory _blackBoxFactory;

        [Header("Login Details")]
        public string Username = "Bob";
        public string Password = "Meanie";

        #endregion

        #region Unity Methods

#if UNITY_EDITOR
        private void OnValidate()
        {
            _blackBoxFactory = GetComponent<BlackBoxFactory>();
        }
#endif
        #endregion

        #region Server

        public override void ServerSetup(NetworkServer server)
        {
            server.MessageHandler.RegisterHandler<AuthData>(OnServerReceivedEncryptedData);
        }

        public override void ServerAuthenticate(INetworkPlayer player)
        {
            // wait for Message from client
        }

        /// <summary>
        ///     Default implementation is to accept everyone.
        ///     Override this function to do your own checks.
        /// </summary>
        /// <param name="player">The player who sent login details.</param>
        /// <param name="message">The login details coming in.</param>
        protected virtual void OnServerReceivedEncryptedData(INetworkPlayer player, AuthData message)
        {
            if (Logger.LogEnabled())
                Logger.LogFormat(LogType.Log,
                    $"Login Request: Username: {message.Username} Password: {message.Password}.");

            _blackBoxFactory.ServerBlackBoxEncryption.Send(player, new AuthResponse {Message = "You passed authentication.", Success = true});

            ServerAccept(player);
        }

        #endregion

        #region Common

        private void OnKeyGenerated(INetworkPlayer player)
        {
            _blackBoxFactory.ClientBlackBoxEncryption.Send(player, new AuthData { Password = Password, Username = Username });
        }

        #endregion

        #region Client

        public override void ClientSetup(NetworkClient client)
        {
            client.MessageHandler.RegisterHandler<AuthResponse>(OnMessageResponse);

            _blackBoxFactory.OnClientGeneratedSharedKey.AddListener(OnKeyGenerated);
        }

        public override void ClientAuthenticate(INetworkPlayer player)
        {
            // NOOP. We need to wait for client and server to share keys to generate a public shared key.
        }

        /// <summary>
        ///     Default implementation is setup to accept or reject user based on what server sends back.
        /// </summary>
        /// <param name="player">The player who sent the message.</param>
        /// <param name="message">The message data we received.</param>
        protected virtual void OnMessageResponse(INetworkPlayer player, AuthResponse message)
        {
            if (Logger.LogEnabled()) Logger.LogFormat(LogType.Log, $"Authentication Success: {message.Message}");

            if (message.Success)
            {
                ClientAccept(player);
            }
            else
            {
                ClientReject(player);
            }
        }

        #endregion
    }
}
