using DoDo.Core;
using DoDo.Terrain;
using System;
using Unity.Netcode;
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
            if (IsLocalPlayer)
            {
                TerrainGenerator.Instance.ReinitChunkHolder();
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

        void TerrainGenerator_OnTerrainCreationStarted(object sender, System.EventArgs e)
        {
            //gameObject.GetComponent<Rigidbody>().useGravity = true;
            //gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
        void TerrainGenerator_OnTerrainCreationFinished(object sender, System.EventArgs e)
        {
            //gameObject.GetComponent<Rigidbody>().useGravity = true;
            //gameObject.GetComponent<Rigidbody>().isKinematic = false;
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

            playerNetworkObject = GetComponent<NetworkObject>();
            playerData = MultiplayerManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
            playerIndex = MultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId);
            playerVisual.SetPlayerColor(GameManager.Instance.GetPlayerColor(playerData.colorId));

            //MatchManager.Instance.SpawnPlayerOnPosition(playerNetworkObject, playerIndex);
            TerrainGenerator.Instance.OnTerrainCreationStarted += TerrainGenerator_OnTerrainCreationStarted;
            TerrainGenerator.Instance.OnTerrainCreationFinished += TerrainGenerator_OnTerrainCreationFinished;

            //OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

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