using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyNameInputField;

    /*******************************************/
    /*              Unity Methods              */
    /*******************************************/

    /*******************************************/
    /*             Private Methods             */
    /*******************************************/

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

    public void PublicLobby()
    {
        LobbyManager.Instance.CreateLobby(lobbyNameInputField.text, false);
    }
    public void PrivateLobby()
    {
        LobbyManager.Instance.CreateLobby(lobbyNameInputField.text, true);
    }
}