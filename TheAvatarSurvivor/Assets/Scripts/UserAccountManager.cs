using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserAccountManager : MonoBehaviour
{
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";

    private string loggedInUsername = "";

    public static UserAccountManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        //loggedInUsername = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(100, 1000));

        //Destroy(gameObject);
    }

    //public void LogIn(Text username)
    //{
    //    if (username.text == "") return;

    //    loggedInUsername = username.text;
    //    Debug.Log($"<color=blue>Username : {username.text}</color>");

    //    usernameTextDisplay.text = "Welcome " + username.text;

    //    usernamePanel.SetActive(false);
    //    connectionPanel.SetActive(true);
    //}


    public string GetPlayerName()
    {
        return loggedInUsername;
    }

    public void SetPlayerName(string playerName)
    {
        loggedInUsername = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }
}
