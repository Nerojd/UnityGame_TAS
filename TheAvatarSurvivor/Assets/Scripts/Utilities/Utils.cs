using UnityEngine;

public enum SceneType
{
    Custom,
    BattleField,
    Team

}
public class Utils
{
    public static string remoteLayerName = "RemotePlayer";

    public static string dontDrawLayerName = "DontDraw";

    public static void SetLayerRecurively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecurively(child.gameObject, newLayer);
        }
    }
}
