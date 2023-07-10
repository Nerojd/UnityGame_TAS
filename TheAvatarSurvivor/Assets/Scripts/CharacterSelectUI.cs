using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    private void Start()
    {
        Lobby lobby = LobbyManager.Instance.GetLobby();

        lobbyNameText.text = "Lobby : " + lobby.Name;
        lobbyCodeText.text = lobby.LobbyCode;
    }

    public void MainMenuButton()
    {
        LobbyManager.Instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        Loader.Load(Loader.Scene.MainMenuScene);
    }
    public void ReadyButton()
    {
        GameManager.Instance.SetPlayerReady();
    }
}