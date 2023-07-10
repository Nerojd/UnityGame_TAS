using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class UsernamePanelUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInputField;

    /*******************************************/
    /*              Unity Methods              */
    /*******************************************/
    private void Start()
    {
        if (UserAccountManager.Instance.GetPlayerName() != "")
        {
            gameObject.SetActive(false);
        }
    }
    /*******************************************/
    /*             Private Methods             */
    /*******************************************/

    /******************************************/
    /*             Public Methods             */
    /******************************************/
    public void SetPlayerName()
    {
        UserAccountManager.Instance.SetPlayerName(playerNameInputField.text);
        gameObject.SetActive(false);
    }
}
