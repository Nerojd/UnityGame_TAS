using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPublicUI : MonoBehaviour
{
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyItem;

    /*******************************************/
    /*              Unity Methods              */
    /*******************************************/
    private void Start()
    {
        LobbyManager.Instance.OnLobbyListChanged += UpdateLobby_OnLobbyListChanged;
        UpdateLobbyList(new List<Lobby>());
    }

    /*******************************************/
    /*             Private Methods             */
    /*******************************************/
    private void UpdateLobby_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        if (lobbyContainer != null)
        {
            foreach (Transform child in lobbyContainer)
            {
                if (child == lobbyItem) continue;
                Destroy(child.gameObject);
            }
        }

        if (lobbyList == null) return;

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyItem, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }

    /******************************************/
    /*             Public Methods             */
    /******************************************/
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void JoinLobby()
    {
        LobbyManager.Instance.QuickJoin();
    }
}