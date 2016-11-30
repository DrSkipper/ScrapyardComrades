using UnityEngine;

public class ParallaxLayerController : MonoBehaviour
{
    public ParallaxQuadGroup CurrentLayerVisual;
    public ParallaxQuadGroup PreviousLayerVisual;

    public void TransitionToNewLayer(SCParallaxLayer layer)
    {
        this.PreviousLayerVisual.UpdateWithMesh(this.CurrentLayerVisual.MeshFilter.mesh);
        this.CurrentLayerVisual.CreateMeshForLayer(layer);
    }
}
