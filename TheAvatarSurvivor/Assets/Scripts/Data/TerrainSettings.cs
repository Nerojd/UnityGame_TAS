using UnityEngine;

[CreateAssetMenu(menuName = "ComputeShaderSettings")]
public class ComputeShaderSettings : ScriptableObject
{
    public bool autoUpdate = true;

    /* Noise Settings */
    public ComputeShader densityComputeShader;

    /* Voxel Settings */
    public ComputeShader meshComputeShader;

    /* Terraform Shader */
    public ComputeShader terraformComputeShader;

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