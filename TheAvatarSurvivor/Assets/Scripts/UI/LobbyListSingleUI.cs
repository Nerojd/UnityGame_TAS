using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;

    private Lobby lobby;

    /*******************************************/
    /*              Unity Methods              */
    /*******************************************/
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            LobbyManager.Instance.JoinWithId(lobby.Id);
        });
    }

    /*******************************************/
    /*             Private Methods             */
    /*******************************************/

    /******************************************/
    /*             Public Methods             */
    /******************************************/
    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
    }

}