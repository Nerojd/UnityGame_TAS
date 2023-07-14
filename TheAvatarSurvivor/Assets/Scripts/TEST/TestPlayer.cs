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
            TestTerrainGenerator.Instance.InitChunkOnServer();
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
        TestTerrainGenerator.Instance.InitChunkOnServer();
    }
}
