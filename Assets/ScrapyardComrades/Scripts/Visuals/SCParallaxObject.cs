using UnityEngine;

public class SCParallaxObject : VoBehavior
{
    public float ParallaxRatio = 0.0f;

    void OnSpawn()
    {
        if (this.ParallaxRatio > 0.001f && ParallaxManager.Instance != null)
        {
            ParallaxManager.Instance.AddObject(this);
        }
    }

    void OnReturnToPool()
    {
        if (this.ParallaxRatio > 0.001f && ParallaxManager.Instance != null)
        {
            ParallaxManager.Instance.RemoveObject(this);
        }
    }
}
