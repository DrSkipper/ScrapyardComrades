using UnityEngine;

public class ParallaxLayerController : MonoBehaviour
{
    public ParallaxQuadGroup CurrentLayerVisual;
    public ParallaxQuadGroup PreviousLayerVisual;

    public void TransitionToNewLayer(SCParallaxLayer layer)
    {
        if (layer.Sprite != this.CurrentLayerVisual.MostRecentSprite)
        {
            this.PreviousLayerVisual.UpdateWithMesh(this.CurrentLayerVisual.MeshFilter.mesh);
            this.CurrentLayerVisual.CreateMeshForLayer(layer);
        }
    }
}
