using Mirage;
using UnityEngine;

namespace BlackBox
{
    public class BlackBox : MonoBehaviour
    {
        #region Fields

        private BlackBoxEncryption _blackBoxEncryption;
        private NetworkServer _server;
        private NetworkClient _client;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _server = GetComponent<NetworkServer>();
            _client = GetComponent<NetworkClient>();

            if (_server && _server.Active)
                _server.Started.AddListener(OnServerStarted);

            if(_client && !_client.IsLocalClient && _client.Active)
                _client.Started.AddListener(OnClientStarted);
        }

        #endregion

        #region Client

        /// <summary>
        /// 
        /// </summary>
        private void OnClientStarted()
        {
            _blackBoxEncryption = new BlackBoxEncryption(_client.MessageHandler);
        }

        #endregion

        #region Server

        /// <summary>
        ///     Server has started up let's assign server handler now to encryption.
        /// </summary>
        private void OnServerStarted()
        {
            _blackBoxEncryption = new BlackBoxEncryption(_server.MessageHandler);
        }

        #endregion
    }
}
