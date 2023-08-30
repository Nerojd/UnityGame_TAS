using DoDo.Core;
using DoDo.Terrain;
using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace DoDo.Player
{
    public class Player : NetworkBehaviour
    {
        public static event EventHandler OnAnyPlayerSpawned;

        [SerializeField] private PlayerVisual playerVisual;
        NetworkObject playerNetworkObject = null;
        PlayerData playerData;
        int playerIndex = -1;

        private bool _isDead = false;
        public bool isDead
        {
            get { return _isDead; }
            protected set { _isDead = value; }
        }

        // TODO in PlayerHealth
        [SerializeField] private float maxHealth = 100f;
        public float currentHealth;
        public float GetHealthPercent()
        {
            return currentHealth / maxHealth;
        }


        public static Player LocalInstance { get; private set; }

        /*******************************************/
        /*              Unity Methods              */
        /*******************************************/
        private void Start()
        {
            playerNetworkObject = gameObject.GetComponent<NetworkObject>();
            playerData = MultiplayerManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
            playerIndex = MultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId);

            TerrainGenerator.Instance.OnTerrainCreationStarted += TerrainGenerator_OnTerrainCreationStarted;
            TerrainGenerator.Instance.OnTerrainCreationFinished += TerrainGenerator_OnTerrainCreationFinished;

            if (IsOwner)
            {
                // Teleport player to a different spawn location
                // To be call by the owner to avoid non-authoritative call
                Vector3 playerPosition = MatchManager.Instance.GetSpawnPosition(MultiplayerManager.Instance.GetPlayerDataIndexFromClientId(playerData.clientId));
                gameObject.GetComponent<NetworkTransform>().Teleport(playerPosition, transform.rotation, transform.localScale);
            }

            if (IsLocalPlayer)
            {
                if (playerData.clientId == 0)
                {
                    MatchManager.OnAllClientPlayerSpawned += MatchManager_OnAllClientPlayerSpawned;
                }

                MatchManager.Instance.NotifyServerPlayerHasSpawned();
            }
        }

        /*******************************************/
        /*             Private Methods             */
        /*******************************************/
        private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
        {
            if (clientId == OwnerClientId)
            {
                ;
            }
        }

        private void MatchManager_OnAllClientPlayerSpawned(object sender, EventArgs e)
        {
            TerrainGenerator.Instance.InitChunkOnServer();
        }

        void TerrainGenerator_OnTerrainCreationStarted(object sender, System.EventArgs e)
        {
            gameObject.GetComponent<Rigidbody>().useGravity = false;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
        void TerrainGenerator_OnTerrainCreationFinished(object sender, System.EventArgs e)
        {
            gameObject.GetComponent<Rigidbody>().useGravity = true;
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }

        /******************************************/
        /*             Public Methods             */
        /******************************************/
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                LocalInstance = this;
            }

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            }
        }

        public void Setup()
        {

        }
    }
}