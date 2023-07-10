using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private Color readyColor;
    [SerializeField] private Color notReadyColor;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Button kickButton;
    [SerializeField] private TextMeshPro playerNameText;

    /*******************************************/
    /*              Unity Methods              */
    /*******************************************/
    private void Start()
    {
        MultiplayerManager.Instance.OnPlayerDataNetworkListChanged += MultiplayerManager_OnPlayerDataNetworkListChanged;
        GameManager.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;

        kickButton.gameObject.SetActive(NetworkManager.Singleton.IsHost);

        UpdatePlayer();
    }
    private void OnDestroy()
    {
        MultiplayerManager.Instance.OnPlayerDataNetworkListChanged -= MultiplayerManager_OnPlayerDataNetworkListChanged;
    }

    /*******************************************/
    /*             Private Methods             */
    /*******************************************/
    private void CharacterSelectReady_OnReadyChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void MultiplayerManager_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (MultiplayerManager.Instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();

            PlayerData playerData = MultiplayerManager.Instance.GetPlayerDataFromPlayerIndex(playerIndex);

            TextMeshPro readyTMP = readyGameObject.GetComponent<TextMeshPro>();
            if (readyTMP != null)
            {
                if (GameManager.Instance.IsPlayerReady(playerData.clientId))
                {
                    readyTMP.text = "Ready";
                    readyTMP.color = readyColor;
                }
                else
                {
                    readyTMP.text = "Not Ready";
                    readyTMP.color = notReadyColor;
                }
            }


            playerNameText.text = playerData.playerName.ToString();

            playerVisual.SetPlayerColor(GameManager.Instance.GetPlayerColor(playerData.colorId));
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    /******************************************/
    /*             Public Methods             */
    /******************************************/
    public void KickButton()
    {
        PlayerData playerData = MultiplayerManager.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
        LobbyManager.Instance.KickPlayer(playerData.playerId.ToString());
        MultiplayerManager.Instance.KickPlayer(playerData.clientId);
    }
}