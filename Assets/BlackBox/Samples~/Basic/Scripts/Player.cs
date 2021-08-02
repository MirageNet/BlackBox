using Mirage;
using UnityEngine;
using UnityEngine.UI;

namespace BlackBox.Examples.Basic
{
    public class Player : NetworkBehaviour
    {
        [Header("Player Components")]
        public RectTransform rectTransform;
        public Image image;

        [Header("Child Text Objects")]
        public Text playerNameText;
        public Text playerDataText;

        // These are set in OnStartServer and used in OnStartClient
        [SyncVar]
        int playerNo;

        [SyncVar]
        Color playerColor;

        private static int playerCounter = 1;

        private BlackBoxFactory _blk;

        private static int GetNextPlayerId()
        {
            return playerCounter++;
        }

        void Awake()
        {
            _blk = FindObjectOfType<BlackBoxFactory>();

            NetIdentity.OnStartServer.AddListener(OnStartServer);
            NetIdentity.OnStartClient.AddListener(OnStartClient);
            NetIdentity.OnStartLocalPlayer.AddListener(OnStartLocalPlayer);
        }

        [NetworkMessage]
        public struct EncryptedMessage
        {
            public int Message;
        }

        // This is called by the hook of playerData SyncVar above
        private void OnPlayerDataChanged(INetworkPlayer player, EncryptedMessage message)
        {
            // Show the data in the UI
            playerDataText.text = $"Decrypted Data: {message.Message:000}";
        }

        // This fires on server when this player object is network-ready
        public void OnStartServer()
        {
            // Set SyncVar values
            playerNo = GetNextPlayerId();
            playerColor = Random.ColorHSV(0f, 1f, 0.9f, 0.9f, 1f, 1f);

            // Start generating updates
            InvokeRepeating(nameof(UpdateData), 1, 1);
        }

        // This only runs on the server, called from OnStartServer via InvokeRepeating
        [Server(error = false)]
        void UpdateData()
        {
            var message = new EncryptedMessage { Message = Random.Range(100, 1000)};

            _blk.BlackBoxEncryption.Send(NetIdentity.ConnectionToClient, message);
        }

        // This fires on all clients when this player object is network-ready
        public void OnStartClient()
        {
            // Make this a child of the layout panel in the Canvas
            transform.SetParent(GameObject.Find("PlayersPanel").transform);

            // Calculate position in the layout panel
            int x = 100 + ((playerNo % 4) * 150);
            int y = -170 - ((playerNo / 4) * 80);
            rectTransform.anchoredPosition = new Vector2(x, y);

            // Apply SyncVar values
            playerNameText.color = playerColor;
            playerNameText.text = $"Player {playerNo:00}";

            NetIdentity.ClientObjectManager.Client.MessageHandler.RegisterHandler<EncryptedMessage>(OnPlayerDataChanged);
        }

        // This only fires on the local client when this player object is network-ready
        public void OnStartLocalPlayer()
        {
            // apply a shaded background to our player
            image.color = new Color(1f, 1f, 1f, 0.1f);
        }
    }
}
