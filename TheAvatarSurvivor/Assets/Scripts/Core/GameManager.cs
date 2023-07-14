using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private List<Color> playerColorList;
    public int numberOfPlayersAlive = 0;

    //public MatchSettings matchSettings;
    //public int currentGameMode;

    public delegate void OnPlayerKilledCallback(string player, string source);
    public OnPlayerKilledCallback onPlayerKilledCallback;

    private Dictionary<ulong, bool> playerReadyDictionary;
    public event EventHandler OnReadyChanged;

    public static GameManager Instance { get; private set; }

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

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }


    /*******************************************/
    /*             Private Methods             */
    /*******************************************/
    // SetPlyerColorWithFirstUnusedColorId()
    //{
    //    GetFirstUnusedColorId();
    //}

    /******************************************/
    /*             Public Methods             */
    /******************************************/
    public void PlayerAlive(bool isNewAlive)
    {
        if (isNewAlive)
        {
            numberOfPlayersAlive++;
        }
        else
        {
            numberOfPlayersAlive--;
        }
        Debug.Log("numberOfPlayers alive : " + numberOfPlayersAlive);

    }

    /************************/
    /*    PLAYERS COLORS    */
    /************************/
    public Color GetPlayerColor(int colorId)
    {
        return playerColorList[colorId];
    }

    public int GetPlayerColorCount()
    {
        return playerColorList.Count;
    }

    public void ChangePlayerColorId(int colorId)
    {
        if (!IsColorAvailable(colorId))
        {
            // Color not available
            return;
        }
        MultiplayerManager.Instance.ChangePlayerColorId(colorId);
    }

    private bool IsColorAvailable(int colorId)
    {
        bool lStatus = true;
        // TODO : Network list into list
        NetworkList<PlayerData> tempList = MultiplayerManager.Instance.GetPlayerDataNetworkList();
        foreach (PlayerData playerData in tempList)
        {
            if (playerData.colorId == colorId)
            {
                // Already in use
                lStatus = false;
                break;
            }
        }
        return lStatus;
    }

    public int GetFirstUnusedColorId()
    {
        for (int i = 0; i < playerColorList.Count; i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }
        return -1;
    }

    /***********************/
    /*    PLAYERS READY    */
    /***********************/
    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        bool readyState;
        if (!playerReadyDictionary.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            readyState = true;
        }
        else
        {
            readyState = !playerReadyDictionary[serverRpcParams.Receive.SenderClientId];
        }
        SetPlayerReadyClientRpc(readyState, serverRpcParams.Receive.SenderClientId);

        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = readyState;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                // This player is NOT ready
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            LobbyManager.Instance.DeleteLobby();
            //Loader.LoadNetwork(Loader.Scene.GameScene);

            // Test Scene
            Loader.LoadNetwork(Loader.Scene.TestScene);
        }
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(bool readyState, ulong clientId)
    {
        playerReadyDictionary[clientId] = readyState;

        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }
}
