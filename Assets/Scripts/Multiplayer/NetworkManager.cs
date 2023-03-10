using System;
using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;

namespace Multiplayer
{
    public enum ServerToClientID : ushort
    {
        playerSpawned = 1,
        playerMovement
    }
    
    public enum ClientToServerID : ushort
    {
        name = 1,
        input
    }
    
    public class NetworkManager : MonoBehaviour
    {
        private static NetworkManager _singleton;

        public static NetworkManager Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }

        public Client Client { get; private set; }

        [SerializeField] private string ip;
        [SerializeField] private ushort port;

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

            Client = new Client();
            Client.Connected += DidConnect;
            Client.ConnectionFailed += FailedToConnect;
            Client.Disconnected += DidDisconnect;
            Client.ClientDisconnected += PlayerLeft;
        }

        private void FixedUpdate()
        {
            Client.Tick();
        }

        private void OnApplicationQuit()
        {
            Client.Disconnect();
        }

        public void Connect()
        {
            Client.Connect($"{ip}:{port}");
        }

        private void DidConnect(object sender, EventArgs e)
        {
            UIManager.Singleton.SendName();
        }
        
        private void FailedToConnect(object sender, EventArgs e)
        {
            UIManager.Singleton.BackToMain();
        }
        
        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
        {
            if (Player.list.TryGetValue(e.Id, out Player player))
                Destroy(player.gameObject);
        }
        
        private void DidDisconnect(object sender, EventArgs e)
        {
            UIManager.Singleton.BackToMain();
            foreach (var player in Player.list.Values)
                Destroy(player.gameObject);
        }
    }
}