using UnityEngine;

[CreateAssetMenu(menuName = "NoiseSettings")]
public class NoiseSettings : ScriptableObject
{
    public bool autoUpdate = true;

    [Header("Noise")]
    public int seed = 1;
    public Vector3 offset = Vector3.zero;
    public int numOctaves = 2;
    public float lacunarity = 2f;
    [Range(0, 1)]
    public float persistence = .4f;
    public float noiseScale = 3.2f;
    public float noiseWeight = 12f;

    [Header("Options")]
    public float floorOffset = 1;
    public float weightMultiplier = 1;

    public Vector4 shaderParams = new(5f, 2f, 0f, 0f);

    private void ValidateValues()
    {
        numOctaves = Mathf.Max(numOctaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistence = Mathf.Clamp01(persistence);
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
