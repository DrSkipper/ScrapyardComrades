using UnityEngine;

public class SCParallaxLayer : ScriptableObject
{
    public const int NUM_RENDER_LAYERS = 5;

    [System.Serializable]
    public enum RenderLayer
    {
        Default = -1,
        Front = 0,
        FrontMid = 1,
        Mid = 2,
        BackMid = 3,
        Back = 4
    }

    public Sprite Sprite;
    public RenderLayer DefaultRenderLayer = RenderLayer.Mid;
    public bool LoopsHorizontally = true;
    public float Height = 0.5f;
}
