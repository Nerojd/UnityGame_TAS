using DoDo.Terrain;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    [SerializeField] private Transform playerPrefab;
    [SerializeField] private List<Transform> spawnPositionList;

    //NetworkVariable<bool> matchJustStarted;

    private int playerSpawnedCount = 0;

    public static MatchManager Instance { get; private set; }

    /*******************************************/
    /*              Unity Methods              */
    /*******************************************/
    private void Awake()
    {
        Instance = this;
        //if (Instance == null)
        //{
        //    Instance = this;
        //}
        //else
        //{
        //    Debug.LogError("More than one GameManager instance in our scene");
        //    // TODO Destroy to avoid multiple instance
        //    return;
        //}
    }

    /*******************************************/
    /*             Private Methods             */
    /*******************************************/
    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
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

    public void NotifyServerPlayerHasSpawned()
    {
        NotifyServerPlayerHasSpawnedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void NotifyServerPlayerHasSpawnedServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerSpawnedCount++;

        bool allClientsHasSpawned = false;
        if (NetworkManager.Singleton.ConnectedClientsIds.Count == playerSpawnedCount)
        {
            allClientsHasSpawned = true;
        }

        if (allClientsHasSpawned)
        {
            TerrainGenerator.Instance.CreateTerrain(); // Should take a ScriptableObject as parameter (create by players during lobby)
        }
    }

    public void SpawnPlayerOnPosition(NetworkObjectReference target, int playerIndex)
    {
        NetworkObject targetObject = target;
        //SpawnPlayerOnPositionServerRpc(targetObject, playerIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayerOnPositionServerRpc(NetworkObjectReference target, int playerIndex)
    {
        NetworkObject targetObject = target;
        targetObject.GetComponent<Transform>().position = GetSpawnPosition(playerIndex);
    }
}
