using UnityEngine;

public class SimpleCameraShaderEffect : MonoBehaviour
{
    public Material Material;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, this.Material);
    }
}
