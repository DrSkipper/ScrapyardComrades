using UnityEngine;

public class PixelizeScreenEffect : MonoBehaviour, IPausable
{
    public float ResolutionMultiplier = 0.5f;

    void Start()
    {
        if (!SystemInfo.supportsImageEffects)
        {
            this.enabled = false;
            return;
        }
    }

    public void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        this.ResolutionMultiplier = Mathf.Clamp(this.ResolutionMultiplier, 0.001f, 1.0f);
        RenderTexture scaled = RenderTexture.GetTemporary(Mathf.Clamp(Mathf.RoundToInt(src.width * this.ResolutionMultiplier), 1, src.width), Mathf.Clamp(Mathf.RoundToInt(src.height * this.ResolutionMultiplier), 1, src.height));
        scaled.filterMode = FilterMode.Point;
        Graphics.Blit(src, scaled);
        Graphics.Blit(scaled, dest);
        RenderTexture.ReleaseTemporary(scaled);
    }
}
