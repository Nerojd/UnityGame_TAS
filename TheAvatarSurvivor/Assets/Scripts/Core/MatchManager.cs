using DoDo.Terrain;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    [SerializeField] private Transform playerPrefab;
    [SerializeField] private List<Transform> spawnPositionList;

    public static event EventHandler OnAllClientPlayerSpawned;

    //NetworkVariable<bool> matchJustStarted;

    int playerSpawnedCount = 0;
    bool allClientsHasSpawned = false;

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

            // Instantiate player on Server and on each clients
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        //autoTestGamePausedState = true;
        playerSpawnedCount--;
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
        // TESTER DE LE REACTIVER (2x plus de joueurs ?) 
        //base.OnNetworkSpawn();

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
    void NotifyServerPlayerHasSpawnedServerRpc()
    {
        playerSpawnedCount++;

        if (NetworkManager.Singleton.ConnectedClientsIds.Count == playerSpawnedCount)
        {
            allClientsHasSpawned = true;
        }

        if (allClientsHasSpawned)
        {
            OnAllClientPlayerSpawned?.Invoke(this, EventArgs.Empty);
        }
    }

    public override void OnDestroy()
    {
    }

}
