using UnityEngine;

[CreateAssetMenu(menuName = "MeshSettings")]
public class MeshSettings : ScriptableObject
{
    public bool autoUpdate = true;

    public string terrainLayer = "Terraform";
    public Vector3Int numChunks = new(5, 5, 5);
    [HideInInspector] public float visibleDstThreshold = 35;
    public float boundsSize = 10;
    [HideInInspector] public float isoLevel = 7;
    [Range(2, 100)] public int numPointsPerAxis = 30;

    private void ValidateValues()
    {
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            UnityEditor.EditorApplication.update += ValidateValues;
        }
    }
#endif
}