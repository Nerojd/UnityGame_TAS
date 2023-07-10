using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class TestMatchManager : NetworkBehaviour
{
    [SerializeField] private Transform playerPrefab;
    [SerializeField] private List<Transform> spawnPositionList;


    public static TestMatchManager Instance { get; private set; }

    /*******************************************/
    /*              Unity Methods              */
    /*******************************************/
    private void Awake()
    {
        Instance = this;
    }

    /*******************************************/
    /*             Private Methods             */
    /*******************************************/
    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            
            // Instantiate player on Server
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

            // Teleport player to a different location
            int index = MultiplayerManager.Instance.GetPlayerDataIndexFromClientId(clientId);
            playerTransform.GetComponent<NetworkTransform>().Teleport(GetSpawnPosition(index), transform.rotation, transform.localScale);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        //autoTestGamePausedState = true;
    }

    public Vector3 GetSpawnPosition(int index)
    {
        return spawnPositionList[index].position;
    }

    /******************************************/
    /*             Public Methods             */
    /******************************************/

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //state.OnValueChanged += State_OnValueChanged;
        //isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }
}
