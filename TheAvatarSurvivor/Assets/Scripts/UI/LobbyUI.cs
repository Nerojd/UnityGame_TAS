using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject[] lobbyPanels;

    private int currentPanelIndex = -1;
    /*******************************************/
    /*              Unity Methods              */
    /*******************************************/

    /*******************************************/
    /*             Private Methods             */
    /*******************************************/
    private void LobbyMenuPanel(int index)
    {
        if (currentPanelIndex != index)
        {
            currentPanelIndex = index;

            for (int i = 0; i < lobbyPanels.Length; i++)
            {
                lobbyPanels[i].SetActive(i == currentPanelIndex);
            }
        }
        else
        {
            lobbyPanels[currentPanelIndex].SetActive(!lobbyPanels[currentPanelIndex].activeSelf);
        }
    }

    /******************************************/
    /*             Public Methods             */
    /******************************************/
    public void MainMenuButton()
    {
        LobbyManager.Instance.LeaveLobby();
        Loader.Load(Loader.Scene.MainMenuScene);
    }
    public void LobbyMenuButton(int buttonIndex)
    {
        LobbyMenuPanel(buttonIndex);
    }
}