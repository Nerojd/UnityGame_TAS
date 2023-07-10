using UnityEngine;

[ExecuteInEditMode]
public class TextureGenerator : MonoBehaviour
{
    [SerializeField] Material mat;
    [SerializeField] Vector4 shaderParams;

    public Material GetMaterial()
    {
        return mat;
    }
}