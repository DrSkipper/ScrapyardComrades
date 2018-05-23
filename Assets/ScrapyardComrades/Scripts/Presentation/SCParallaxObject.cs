using UnityEngine;

public class SCParallaxObject : VoBehavior, IPausable
{
    public float ParallaxRatio = 0.0f;

    void OnSpawn()
    {
        if (this.transform.parent != null)
        {
            localOffset = this.transform.localPosition;
        }

        if (this.ParallaxRatio > 0.001f && ParallaxManager.Instance != null)
        {
            ParallaxManager.Instance.AddObject(this);
        }
    }

    void OnReturnToPool()
    {
        if (this.transform.parent != null)
        {
            this.transform.localPosition = localOffset;
        }

        if (this.ParallaxRatio > 0.001f && ParallaxManager.Instance != null)
        {
            ParallaxManager.Instance.RemoveObject(this);
        }
    }

    private Vector3 localOffset;
}
