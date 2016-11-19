using UnityEngine;

public class SCParallaxLayer : ScriptableObject
{
    [System.Serializable]
    public enum RenderLayer
    {
        Default,
        Front,
        FrontMid,
        Mid,
        BackMid,
        Back
    }

    public Sprite Sprite;
    public RenderLayer DefaultRenderLayer = RenderLayer.Mid;
    public bool LoopsHorizontally = true;
    public bool LoopsVertically = false;
}
