using DoDo.Player;
using DoDo.Terrain;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor.PackageManager;
using UnityEngine;

public class TestPlayer : NetworkBehaviour
{
    NetworkObject playerNetworkObject = null;
    PlayerData playerData;
    int playerIndex = -1;

    public static TestPlayer LocalInstance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        playerNetworkObject = gameObject.GetComponent<NetworkObject>();
        playerData = MultiplayerManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerIndex = MultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId);

        TestTerrainGenerator.Instance.OnTerrainCreationStarted += TerrainGenerator_OnTerrainCreationStarted;
        TestTerrainGenerator.Instance.OnTerrainCreationFinished += TerrainGenerator_OnTerrainCreationFinished;

        if (IsOwner)
        {
            // Teleport player to a different spawn location
            // To be call by the owner to avoid non-authoritative call
            Vector3 playerPosition = TestMatchManager.Instance.GetSpawnPosition(MultiplayerManager.Instance.GetPlayerDataIndexFromClientId(playerData.clientId));
            gameObject.GetComponent<NetworkTransform>().Teleport(playerPosition, transform.rotation, transform.localScale);
        }

        if (IsLocalPlayer)
        {
            if (playerData.clientId == 0)
            {
                TestMatchManager.OnAllClientPlayerSpawned += TestMatchManager_OnAllClientPlayerSpawned;
            }

            TestMatchManager.Instance.NotifyServerPlayerHasSpawned();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsLocalPlayer) return;

        if (Input.GetKeyUp(KeyCode.T))
        {
            TestTerrainGenerator.Instance.ChangeMeshOnServer(transform.position);
        }

    }

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

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId)
        {
            ;
        }
    }

    private void TestMatchManager_OnAllClientPlayerSpawned(object sender, EventArgs e)
    {
        //TestTerrainGenerator.Instance.InitChunkOnServer();
    }

    void TerrainGenerator_OnTerrainCreationStarted(object sender, System.EventArgs e)
    {
        //gameObject.GetComponent<Rigidbody>().useGravity = false;
        //gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }
    void TerrainGenerator_OnTerrainCreationFinished(object sender, System.EventArgs e)
    {
        //gameObject.GetComponent<Rigidbody>().useGravity = true;
        //gameObject.GetComponent<Rigidbody>().isKinematic = false;

        // Get or Set anything after Terrain is created
    }
}
