using DoDo.Terrain;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            TestTerrainGenerator.Instance.SpawnSphereOnServer(transform.position);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        playerNetworkObject = GetComponent<NetworkObject>();
        playerData = MultiplayerManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerIndex = MultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId);

        Debug.Log("owernerclientID : " + NetworkManager.LocalClient.PlayerObject.OwnerClientId);
        
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
}
