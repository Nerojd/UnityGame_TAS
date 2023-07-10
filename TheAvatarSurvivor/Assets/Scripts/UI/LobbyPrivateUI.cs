using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPrivateUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeInputField;

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

    public void JoinLobby()
    {
        LobbyManager.Instance.JoinWithCode(joinCodeInputField.text);
    }
}