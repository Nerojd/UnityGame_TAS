using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool isOn = false;

    [SerializeField] private GameObject canvasButtons;
    [SerializeField] private GameObject canvasSettings;

    [SerializeField] private GameObject PlayerUiObj;

    public void LeaveRoomButton()
    {
        //PlayerUiObj.GetComponent<PlayerUI>().RemovePlayerGameManager();
    }

    public void SettingsButton()
    {
        canvasButtons.SetActive(false);
        canvasSettings.SetActive(true);
    }

    public void ReconfigureMenu()
    {
        canvasButtons.SetActive(true);
        canvasSettings.SetActive(false);
    }
}
