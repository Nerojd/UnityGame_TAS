using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenuScene,
        LoadingScene,
        LobbySelectionScene,
        LobbyScene,
        GameScene,
        TestScene,
    }
    private static Scene targetScene;

    /*******************************************/
    /*              Unity Methods              */
    /*******************************************/

    /*******************************************/
    /*             Private Methods             */
    /*******************************************/

    /******************************************/
    /*             Public Methods             */
    /******************************************/
    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
